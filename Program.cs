using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PickupOrderSystem.API.Endpoints;
using PickupOrderSystem.Application.Interfaces;
using PickupOrderSystem.Application.Interfaces.Repositories;
using PickupOrderSystem.Application.Services;
using PickupOrderSystem.Infrastructure.Data;
using PickupOrderSystem.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IPickupRequestRepository, PickupRequestRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

// Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPickupRequestService, PickupRequestService>();
builder.Services.AddScoped<IDriverVehicleService, DriverVehicleService>();

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

app.MapAuthEndpoints();
app.MapPickupRequestEndpoints();
app.MapDriverVehicleEndpoints();

app.MapGet("/colaborador/ping", () => "Acesso permitido: Colaborador")
    .RequireAuthorization("Colaborador")
    .WithOpenApi();

app.MapGet("/cliente/ping", () => "Acesso permitido: Cliente")
    .RequireAuthorization("Cliente")
    .WithOpenApi();

app.Run();
