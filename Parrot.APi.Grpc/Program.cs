using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Parrot.Application.Auth;
using Parrot.Application.Integrations;
using Parrot.APi.Grpc.Services;
using Parrot.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
JwtSettings jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration is missing.");

if (!builder.Environment.IsDevelopment() &&
    jwtSettings.Secret is "YourSuperSecretKeyThatIsAtLeast32CharactersLong!")
{
    throw new InvalidOperationException(
        "JwtSettings:Secret must be changed from the default placeholder before running in a non-development environment.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddAuthorization();

// gRPC
builder.Services.AddGrpc();

// MongoDB
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(MongoDbSettings.SectionName));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    MongoDbSettings settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMessageRepository, MongoMessageRepository>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<MessageVerificationGrpcService>();
app.MapGet("/", () => "Parrot gRPC — use a gRPC client to connect.");

app.Run();
