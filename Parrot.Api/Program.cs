using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Parrot.Application.Auth;
using Parrot.Application.Email;
using Parrot.Application.Logging;
using Parrot.Application.Models;
using Parrot.Application.Utils;
using Parrot.Infrastructure.Data;
using Parrot.Infrastructure.Email;
using Parrot.Api.Middleware;
using Parrot.Api.Utils;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
JwtSettings jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration is missing.");

if (!builder.Environment.IsDevelopment() &&
    jwtSettings.Secret is "YourSuperSecretKeyThatIsAtLeast32CharactersLong!")
{
    throw new InvalidOperationException(
        "JwtSettings:Secret must be changed from the default placeholder before running in a non-development environment.");
}

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.Zero,
    };
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    static FixedWindowRateLimiterOptions PerIp(int permitLimit, int windowMinutes) =>
        new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(windowMinutes),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
        };

    // 5 registrations per IP per 15 minutes
    options.AddPolicy(RateLimitPolicies.Register, ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => PerIp(5, 15)));

    // 10 login attempts per IP per 15 minutes
    options.AddPolicy(RateLimitPolicies.Login, ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => PerIp(10, 15)));

    // 10 verify-email attempts per IP per 15 minutes
    options.AddPolicy(RateLimitPolicies.VerifyEmail, ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => PerIp(10, 15)));

    // 3 resend-verification emails per IP per 15 minutes
    options.AddPolicy(RateLimitPolicies.ResendVerification, ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => PerIp(3, 15)));

    // 3 forgot-password requests per IP per 15 minutes
    options.AddPolicy(RateLimitPolicies.ForgotPassword, ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => PerIp(3, 15)));

    // 5 reset-password attempts per IP per 15 minutes
    options.AddPolicy(RateLimitPolicies.ResetPassword, ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => PerIp(5, 15)));

    // 5 change-password attempts per IP per 15 minutes
    options.AddPolicy(RateLimitPolicies.ChangePassword, ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => PerIp(5, 15)));
});

// Services
builder.Services.AddScoped<IJwtHelper, JwtHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<RequestContext>();
builder.Services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<RequestContext>());
builder.Services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));

// Controllers & OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// Global Exception Handler (must be first in pipeline)
app.UseGlobalExceptionHandler();

// Request context: populates IRequestContext and opens the structured log scope
app.UseRequestContext();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
