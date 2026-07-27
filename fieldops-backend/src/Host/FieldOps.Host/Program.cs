using FieldOps.Modules.Configuracion.Application.Abstractions;
using FieldOps.Modules.Configuracion.Application.Commands.CreateCompany;
using FieldOps.Modules.Configuracion.Infrastructure.Persistence;
using FieldOps.Modules.Configuracion.Infrastructure.Repositories;
using FieldOps.Shared.Outbox;
using FieldOps.Shared.Tenancy;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Autenticación (13-Security/README.md: JWT + 2FA obligatorio desde
//     el MVP — el segundo factor se valida en el flujo de login, antes
//     de emitir este token; aquí solo se valida el token ya emitido). ---
builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(); // opciones concretas (issuer, audiencia, clave) — ver appsettings

builder.Services.AddAuthorization();

// --- MediatR: registra los Handlers de cada módulo. Un módulo nuevo
//     agrega su propio assembly aquí, nunca modifica los de otro. ---
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<CreateCompanyCommand>());

// --- Tenancy: un TenantContext por request (04-Architecture/README.md
//     regla #4 — se resuelve UNA vez, en el middleware). ---
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

// --- Outbox: relay compartido (08-Backend/README.md — Postgres Outbox +
//     Redis Streams). ---
builder.Services.AddHostedService<OutboxRelayBackgroundService>();
// TODO: registrar la implementación concreta de IOutboxWriter (escribe
// a la tabla outbox_event dentro de la misma transacción del módulo).

// --- Módulo Configuración: EF Core sobre PostgreSQL + repository. ---
builder.Services.AddDbContext<ConfiguracionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddScoped<IConfiguracionRepository, EfConfiguracionRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();

// Resuelve company_id/user_id UNA sola vez, en el borde
// (04-Architecture/README.md regla #4) — antes de UseAuthorization para
// que las políticas basadas en Company ya tengan el contexto disponible.
app.UseMiddleware<TenantMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
