using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PickupOrderSystem.Application.DTOs;
using PickupOrderSystem.Application.Services;
using PickupOrderSystem.Domain.Enums;
using PickupOrderSystem.Domain.Entities;
using PickupOrderSystem.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Colaborador", policy => policy.RequireRole("Colaborador"))
    .AddPolicy("Cliente", policy => policy.RequireRole("Cliente"))
    .AddPolicy("Motorista", policy => policy.RequireRole("Motorista"));

builder.Services.AddCors(options =>
    options.AddPolicy("ReactDev", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ReactDev");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    var result = await authService.LoginAsync(request);
    return result is null ? Results.Unauthorized() : Results.Ok(result);
})
.WithName("Login")
.WithOpenApi()
.AllowAnonymous();

app.MapGet("/colaborador/ping", () => "Acesso permitido: Colaborador")
    .RequireAuthorization("Colaborador")
    .WithOpenApi();

app.MapGet("/cliente/ping", () => "Acesso permitido: Cliente")
    .RequireAuthorization("Cliente")
    .WithOpenApi();

// ── Pickup Requests ───────────────────────────────────────────────────────────

app.MapGet("/pickup-requests", async (ClaimsPrincipal user, AppDbContext db) =>
{
    var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var role = user.FindFirstValue(ClaimTypes.Role)!;

    var query = db.PickupRequests.Include(r => r.User).AsQueryable();

    if (role == "Cliente")
        query = query.Where(r => r.UserId == userId);

    var items = await query
        .OrderByDescending(r => r.RequestDate)
        .Select(r => new PickupRequestDto(
            r.Id, r.IdentificationNumber, r.User.Name,
            r.Sender, r.PickupAddress, r.Recipient, r.DeliveryAddress,
            r.RequestDate, r.ScheduledPickupDate,
            r.Priority.ToString(), r.Status.ToString(), r.Notes))
        .ToListAsync();

    return Results.Ok(items);
})
.RequireAuthorization()
.WithOpenApi();

app.MapGet("/pickup-requests/{id:guid}", async (Guid id, ClaimsPrincipal user, AppDbContext db) =>
{
    var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var role = user.FindFirstValue(ClaimTypes.Role)!;

    var r = await db.PickupRequests.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
    if (r is null) return Results.NotFound();
    if (role == "Cliente" && r.UserId != userId) return Results.Forbid();

    return Results.Ok(new PickupRequestDto(
        r.Id, r.IdentificationNumber, r.User.Name,
        r.Sender, r.PickupAddress, r.Recipient, r.DeliveryAddress,
        r.RequestDate, r.ScheduledPickupDate,
        r.Priority.ToString(), r.Status.ToString(), r.Notes));
})
.RequireAuthorization()
.WithOpenApi();

app.MapPost("/pickup-requests", async (CreatePickupRequestRequest body, ClaimsPrincipal user, AppDbContext db) =>
{
    if (!Enum.TryParse<Priority>(body.Priority, out var priority))
        return Results.BadRequest("Prioridade inválida.");

    var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    var year = DateTime.UtcNow.Year;
    var prefix = $"COL-{year}-";
    var last = await db.PickupRequests
        .Where(r => r.IdentificationNumber.StartsWith(prefix))
        .OrderByDescending(r => r.IdentificationNumber)
        .Select(r => r.IdentificationNumber)
        .FirstOrDefaultAsync();
    var nextSeq = last is null ? 1 : int.Parse(last[prefix.Length..]) + 1;

    var now = DateTime.UtcNow;
    var request = new PickupRequest
    {
        Id = Guid.NewGuid(),
        IdentificationNumber = $"{prefix}{nextSeq:D4}",
        UserId = userId,
        Sender = body.Sender,
        PickupAddress = body.PickupAddress,
        Recipient = body.Recipient,
        DeliveryAddress = body.DeliveryAddress,
        RequestDate = now,
        ScheduledPickupDate = body.ScheduledPickupDate,
        Priority = priority,
        Status = PickupRequestStatus.Aberta,
        Notes = body.Notes,
        CreatedAt = now,
        UpdatedAt = now
    };

    db.PickupRequests.Add(request);
    await db.SaveChangesAsync();

    return Results.Created($"/pickup-requests/{request.Id}", new { request.Id, request.IdentificationNumber });
})
.RequireAuthorization()
.WithOpenApi();

app.MapPatch("/pickup-requests/{id:guid}/status", async (Guid id, UpdateStatusRequest body, ClaimsPrincipal user, AppDbContext db) =>
{
    if (!Enum.TryParse<PickupRequestStatus>(body.Status, out var newStatus))
        return Results.BadRequest("Status inválido.");

    var r = await db.PickupRequests.FindAsync(id);
    if (r is null) return Results.NotFound();

    r.Status = newStatus;
    r.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.RequireAuthorization("Colaborador")
.WithOpenApi();

app.Run();
