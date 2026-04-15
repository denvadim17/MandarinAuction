using System.Text;
using MandarinAuction.Api.BackgroundServices;
using MandarinAuction.Api.Hubs;
using MandarinAuction.Api.Services;
using MandarinAuction.Application.Abstractions.Persistence;
using MandarinAuction.Application.Abstractions.Services;
using MandarinAuction.Application.Services;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Infrastructure.Data;
using MandarinAuction.Infrastructure.Persistence;
using MandarinAuction.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Combine(builder.Environment.ContentRootPath, "mandarin.db");
var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        string.IsNullOrWhiteSpace(connectionString) ? $"Data Source={dbPath}" : connectionString,
        sqlite => sqlite.MigrationsAssembly("MandarinAuction.Api")));

var emailSettings = new EmailSettings();
builder.Configuration.GetSection("Email").Bind(emailSettings);

builder.Services.AddSingleton(emailSettings);
builder.Services.AddScoped<IAuctionSettingsRepository, AuctionSettingsRepository>();
builder.Services.AddScoped<IMandarinRepository, MandarinRepository>();
builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IAuctionSettingsService, AuctionSettingsService>();
builder.Services.AddScoped<IMandarinService, MandarinService>();
builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ICashbackService, CashbackService>();

builder.Services.AddIdentityCore<AppUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyForMandarinAuction2024!@#$%";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MandarinAuction";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MandarinAuction";

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && context.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<TokenService>();

builder.Services.AddHostedService<MandarinGeneratorService>();
builder.Services.AddHostedService<MandarinCleanupService>();

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    if (await userManager.FindByEmailAsync("admin@mandarin.com") is null)
    {
        var admin = new AppUser
        {
            UserName = "admin",
            Email = "admin@mandarin.com",
            Balance = 999999
        };
        await userManager.CreateAsync(admin, "Admin123");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<AuctionHub>("/hubs/auction");

app.Run();
