using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CatCollectorAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// -------------------- DATABASE (SQLite) --------------------
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlite("Data Source=catcollector.db")
);

// -------------------- AUTH (JWT) --------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "REPLACE_WITH_A_LONG_RANDOM_KEY_AT_LEAST_32_BYTES";
    if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
        throw new InvalidOperationException("Jwt:Key must be at least 32 bytes for HS256.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// -------------------- SERVICES --------------------
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// -------------------- APPLY MIGRATIONS / CREATE DATABASE AUTOMATICALLY --------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    // Prefer migrations for schema updates in existing DBs. If you add a model, run:
    //   dotnet ef migrations add AddGameResult
    //   dotnet ef database update
    // For simple setups, Migrate will apply pending migrations when available.
    db.Database.Migrate();
}

// -------------------- Startup diagnostics (non-sensitive) --------------------
try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var jwtKeyConfig = builder.Configuration["Jwt:Key"] ?? string.Empty;
    var jwtKeyLen = System.Text.Encoding.UTF8.GetByteCount(jwtKeyConfig);
    if (jwtKeyLen < 32)
    {
        logger.LogWarning("Jwt:Key length is {Len} bytes which is too short for HS256; set a key of at least 32 bytes.", jwtKeyLen);
    }
    else
    {
        logger.LogInformation("Jwt:Key length is {Len} bytes (sufficient).", jwtKeyLen);
    }

    // If running in development and a config key is present, warn about development-only key usage
    if (app.Environment.IsDevelopment())
    {
        if (!string.IsNullOrEmpty(jwtKeyConfig) && jwtKeyLen >= 32)
        {
            logger.LogWarning("Using a JWT key from configuration in Development. For production, set Jwt__Key as an environment variable or use a secrets store.");
        }
        else if (jwtKeyLen > 0 && jwtKeyLen < 32)
        {
            logger.LogWarning("Development Jwt:Key is present but too short; set Jwt__Key to at least 32 bytes to avoid runtime errors.");
        }
    }
}
catch { /* ignore logging errors during startup diagnostics */ }

// -------------------- PIPELINE --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
