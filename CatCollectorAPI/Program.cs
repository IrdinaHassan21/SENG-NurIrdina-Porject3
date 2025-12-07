using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CatCollectorAPI.Data;
using CatCollectorAPI.Services;

// Build
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS - allow your frontend origin or allow all for dev
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

// Jwt service
builder.Services.AddSingleton<JwtService>();

// Authentication - JWT Bearer
var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? string.Empty);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = !string.IsNullOrEmpty(configuration["Jwt:Issuer"]),
        ValidateAudience = !string.IsNullOrEmpty(configuration["Jwt:Audience"]),
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Ensure DB created and seed sample data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    db.Database.Migrate();

    // seed if empty
    if (!db.Players.Any())
    {
        db.Players.AddRange(
            new CatCollectorAPI.Models.Player { Name = "Ms. Whiskers", BestScore = 120, GoodCatsCollected = 35, BadCatsCollected = 5, FatCatsCollected = 2 },
            new CatCollectorAPI.Models.Player { Name = "Captain Fluff", BestScore = 240, GoodCatsCollected = 80, BadCatsCollected = 10, FatCatsCollected = 5 },
            new CatCollectorAPI.Models.Player { Name = "Sir Meowsalot", BestScore = 60, GoodCatsCollected = 10, BadCatsCollected = 2, FatCatsCollected = 0 }
        );
        db.SaveChanges();
    }

    if (!db.Users.Any())
    {
        // create admin user: username admin, password Admin123!
        CreatePasswordHash("Admin123!", out byte[] h, out byte[] s);
        db.Users.Add(new AppUser { Username = "admin", PasswordHash = h, PasswordSalt = s, Role = "Admin" });
        db.SaveChanges();
    }
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
{
    using var hmac = new System.Security.Cryptography.HMACSHA512();
    passwordSalt = hmac.Key;
    passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
}
