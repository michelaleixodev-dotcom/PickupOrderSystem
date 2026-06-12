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
            r.Priority.ToString(), r.Status.ToString(), r.Notes, null, null, null))
        .ToListAsync();

    return Results.Ok(items);
})
.RequireAuthorization()
.WithOpenApi();

app.MapGet("/pickup-requests/{id:guid}", async (Guid id, ClaimsPrincipal user, AppDbContext db) =>
{
    var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var role = user.FindFirstValue(ClaimTypes.Role)!;

    var r = await db.PickupRequests
        .Include(r => r.User)
        .Include(r => r.Assignments).ThenInclude(a => a.Driver)
        .Include(r => r.Assignments).ThenInclude(a => a.Vehicle)
        .Include(r => r.StatusHistories)
        .Include(r => r.Occurrences)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (r is null) return Results.NotFound();
    if (role == "Cliente" && r.UserId != userId) return Results.Forbid();

    var active = r.Assignments.FirstOrDefault(a => a.ActualEndDate == null);
    var assignment = active is null ? null : new AssignmentDto(
        active.Id, active.Driver.Name,
        active.Vehicle.LicensePlate, active.Vehicle.Model,
        active.AssignmentDate);

    var statusHistory = r.StatusHistories
        .OrderBy(h => h.ChangedAt)
        .Select(h => new StatusHistoryDto(h.FromStatus, h.ToStatus, h.ChangedAt, h.ChangedByNameSnapshot))
        .ToList();

    var occurrences = r.Occurrences
        .OrderByDescending(o => o.OccurrenceDate)
        .Select(o => new OccurrenceDto(o.Id, o.Type.ToString(), o.Description, o.OccurrenceDate, o.RegisteredByNameSnapshot, o.Resolved, o.ResolutionNotes))
        .ToList();

    return Results.Ok(new PickupRequestDto(
        r.Id, r.IdentificationNumber, r.User.Name,
        r.Sender, r.PickupAddress, r.Recipient, r.DeliveryAddress,
        r.RequestDate, r.ScheduledPickupDate,
        r.Priority.ToString(), r.Status.ToString(), r.Notes,
        assignment, statusHistory, occurrences));
})
.RequireAuthorization()
.WithOpenApi();

app.MapPost("/pickup-requests", async (CreatePickupRequestRequest body, ClaimsPrincipal user, AppDbContext db) =>
{
    if (!Enum.TryParse<Priority>(body.Priority, out var priority))
        return Results.BadRequest("Prioridade inválida.");

    var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";

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
    db.StatusHistories.Add(new StatusHistory
    {
        Id = Guid.NewGuid(),
        PickupRequestId = request.Id,
        FromStatus = null,
        ToStatus = PickupRequestStatus.Aberta.ToString(),
        ChangedAt = now,
        ChangedById = userId,
        ChangedByNameSnapshot = userName
    });

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

    var allowed = r.Status switch
    {
        PickupRequestStatus.Aberta            => new[] { PickupRequestStatus.Cancelada },
        PickupRequestStatus.Atribuida         => new[] { PickupRequestStatus.EmAndamento, PickupRequestStatus.Cancelada },
        PickupRequestStatus.EmAndamento       => new[] { PickupRequestStatus.Concluida, PickupRequestStatus.FalhaNaColeta, PickupRequestStatus.Cancelada },
        PickupRequestStatus.FalhaNaColeta     => new[] { PickupRequestStatus.AguardandoDecisao, PickupRequestStatus.Cancelada },
        PickupRequestStatus.AguardandoDecisao => new[] { PickupRequestStatus.Atribuida, PickupRequestStatus.Cancelada },
        _                                     => Array.Empty<PickupRequestStatus>()
    };

    if (!allowed.Contains(newStatus))
        return Results.BadRequest($"Transição de '{r.Status}' para '{newStatus}' não é permitida.");

    var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
    var now = DateTime.UtcNow;

    db.StatusHistories.Add(new StatusHistory
    {
        Id = Guid.NewGuid(),
        PickupRequestId = id,
        FromStatus = r.Status.ToString(),
        ToStatus = newStatus.ToString(),
        ChangedAt = now,
        ChangedById = userId,
        ChangedByNameSnapshot = userName
    });

    r.Status = newStatus;
    r.UpdatedAt = now;
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.RequireAuthorization("Colaborador")
.WithOpenApi();

// ── Assignment ────────────────────────────────────────────────────────────────

app.MapGet("/drivers", async (AppDbContext db) =>
{
    var drivers = await db.Users
        .Where(u => u.Role == UserRole.Motorista && u.Active)
        .OrderBy(u => u.Name)
        .Select(u => new { u.Id, u.Name })
        .ToListAsync();
    return Results.Ok(drivers);
})
.RequireAuthorization("Colaborador")
.WithOpenApi();

app.MapGet("/vehicles", async (AppDbContext db) =>
{
    var vehicles = await db.Vehicles
        .Where(v => v.Active)
        .OrderBy(v => v.Model)
        .Select(v => new { v.Id, v.Model, v.LicensePlate })
        .ToListAsync();
    return Results.Ok(vehicles);
})
.RequireAuthorization("Colaborador")
.WithOpenApi();

app.MapPost("/pickup-requests/{id:guid}/assign", async (Guid id, AssignRequest body, ClaimsPrincipal user, AppDbContext db) =>
{
    var r = await db.PickupRequests
        .Include(r => r.Assignments)
        .FirstOrDefaultAsync(r => r.Id == id);
    if (r is null) return Results.NotFound();

    var assignable = new[] { PickupRequestStatus.Aberta, PickupRequestStatus.AguardandoDecisao };
    if (!assignable.Contains(r.Status))
        return Results.BadRequest("Apenas solicitações Abertas ou Aguardando Decisão podem ser atribuídas.");

    var driver = await db.Users.FirstOrDefaultAsync(u => u.Id == body.DriverId && u.Role == UserRole.Motorista && u.Active);
    if (driver is null) return Results.BadRequest("Motorista inválido.");

    var vehicle = await db.Vehicles.FirstOrDefaultAsync(v => v.Id == body.VehicleId && v.Active);
    if (vehicle is null) return Results.BadRequest("Veículo inválido.");

    var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
    var now = DateTime.UtcNow;

    var activeAssignment = r.Assignments.FirstOrDefault(a => a.ActualEndDate == null);
    if (activeAssignment is not null)
    {
        activeAssignment.ActualEndDate = now;
        activeAssignment.UpdatedAt = now;
    }

    db.Assignments.Add(new Assignment
    {
        Id = Guid.NewGuid(),
        PickupRequestId = id,
        DriverId = body.DriverId,
        VehicleId = body.VehicleId,
        AssignmentDate = now,
        CreatedAt = now,
        UpdatedAt = now
    });

    db.StatusHistories.Add(new StatusHistory
    {
        Id = Guid.NewGuid(),
        PickupRequestId = id,
        FromStatus = r.Status.ToString(),
        ToStatus = PickupRequestStatus.Atribuida.ToString(),
        ChangedAt = now,
        ChangedById = userId,
        ChangedByNameSnapshot = userName
    });

    r.Status = PickupRequestStatus.Atribuida;
    r.UpdatedAt = now;

    await db.SaveChangesAsync();
    return Results.NoContent();
})
.RequireAuthorization("Colaborador")
.WithOpenApi();

// ── Occurrences ───────────────────────────────────────────────────────────────

app.MapPost("/pickup-requests/{id:guid}/occurrences", async (Guid id, CreateOccurrenceRequest body, ClaimsPrincipal user, AppDbContext db) =>
{
    if (!Enum.TryParse<OccurrenceType>(body.Type, out var type))
        return Results.BadRequest("Tipo de ocorrência inválido.");

    var r = await db.PickupRequests.FindAsync(id);
    if (r is null) return Results.NotFound();

    var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
    var now = DateTime.UtcNow;

    var occurrence = new Occurrence
    {
        Id = Guid.NewGuid(),
        PickupRequestId = id,
        Type = type,
        Description = body.Description,
        OccurrenceDate = now,
        RegisteredById = userId,
        RegisteredByNameSnapshot = userName,
        Resolved = false,
        CreatedAt = now,
        UpdatedAt = now
    };

    db.Occurrences.Add(occurrence);
    await db.SaveChangesAsync();

    return Results.Created($"/pickup-requests/{id}/occurrences/{occurrence.Id}",
        new OccurrenceDto(occurrence.Id, occurrence.Type.ToString(), occurrence.Description,
            occurrence.OccurrenceDate, occurrence.RegisteredByNameSnapshot, false, null));
})
.RequireAuthorization("Colaborador")
.WithOpenApi();

app.MapPost("/pickup-requests/{id:guid}/fail", async (Guid id, RegisterFailureRequest body, ClaimsPrincipal user, AppDbContext db) =>
{
    if (!Enum.TryParse<OccurrenceType>(body.Type, out var type))
        return Results.BadRequest("Tipo de ocorrência inválido.");

    var r = await db.PickupRequests.FindAsync(id);
    if (r is null) return Results.NotFound();
    if (r.Status != PickupRequestStatus.EmAndamento)
        return Results.BadRequest("Apenas pedidos Em Andamento podem registrar falha.");

    var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
    var now = DateTime.UtcNow;

    db.Occurrences.Add(new Occurrence
    {
        Id = Guid.NewGuid(),
        PickupRequestId = id,
        Type = type,
        Description = body.Description,
        OccurrenceDate = now,
        RegisteredById = userId,
        RegisteredByNameSnapshot = userName,
        Resolved = false,
        CreatedAt = now,
        UpdatedAt = now
    });

    db.StatusHistories.Add(new StatusHistory
    {
        Id = Guid.NewGuid(),
        PickupRequestId = id,
        FromStatus = r.Status.ToString(),
        ToStatus = PickupRequestStatus.FalhaNaColeta.ToString(),
        ChangedAt = now,
        ChangedById = userId,
        ChangedByNameSnapshot = userName
    });

    r.Status = PickupRequestStatus.FalhaNaColeta;
    r.UpdatedAt = now;

    await db.SaveChangesAsync();
    return Results.NoContent();
})
.RequireAuthorization("Colaborador")
.WithOpenApi();

app.Run();
