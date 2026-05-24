using System.Text;
using EmailSender.Api.Middleware;
using EmailSender.Api.Security;
using EmailSender.Application.Configuration;
using EmailSender.Application.Interfaces;
using EmailSender.Application.Services;
using EmailSender.Infrastructure;
using EmailSender.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Configuration["Google:ClientId"] ??= builder.Configuration["GOOGLE_CLIENT_ID"];
builder.Configuration["Google:ClientSecret"] ??= builder.Configuration["GOOGLE_CLIENT_SECRET"];
builder.Configuration["Google:RedirectUri"] ??= builder.Configuration["GOOGLE_REDIRECT_URI"];
builder.Configuration["Jwt:Secret"] ??= builder.Configuration["JWT_SECRET"];
builder.Configuration["Jwt:Issuer"] ??= builder.Configuration["JWT_ISSUER"];
builder.Configuration["Jwt:Audience"] ??= builder.Configuration["JWT_AUDIENCE"];
builder.Configuration["Jwt:ExpiresMinutes"] ??= builder.Configuration["JWT_EXPIRES_MINUTES"];
builder.Configuration["ConnectionStrings:DefaultConnection"] ??= builder.Configuration["DATABASE_URL"];

builder.Services.AddOptions<GoogleOAuthOptions>()
    .Bind(builder.Configuration.GetSection(GoogleOAuthOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId), "GOOGLE_CLIENT_ID is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.ClientSecret), "GOOGLE_CLIENT_SECRET is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.RedirectUri), "GOOGLE_REDIRECT_URI is required.")
    .ValidateOnStart();

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Secret) && options.Secret.Length >= 32, "JWT_SECRET must be at least 32 characters.")
    .ValidateOnStart();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
    {
        var configuredOrigins = builder.Configuration["FRONTEND_ORIGIN"];
        var origins = string.IsNullOrWhiteSpace(configuredOrigins)
            ? [
                "http://localhost:5173",
                "http://localhost:5174",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:5174"
            ]
            : configuredOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Email Sender API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
        }
    });
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EmailSenderDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("ReactFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
