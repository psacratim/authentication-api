using System.Text;
using AuthenticationAPI.Common;
using AuthenticationAPI.Common.Interfaces;
using AuthenticationAPI.Database;
using AuthenticationAPI.Database.Entities;
using AuthenticationAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Starting connection with database
var connectionStr = builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Default connection not found in appsettings.json.");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionStr).UseSnakeCaseNamingConvention();
});

// Enabling essentials components in all application.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Enabling all services from application.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();

// Enabling custom configurations.
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Configuring JWT
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? throw new InvalidOperationException("Jwt configuration not found in appsettings.json");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
}).AddJwtBearer(options =>
{
    // Disable automatic map from short to long names in Claims Key.
    options.MapInboundClaims = false;

    // Configuring how validation works.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,

        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.IssuerSigningKey)),

        ClockSkew = TimeSpan.Zero
    };

    // Listening OnMessageReceived to add access-token in context.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("X-Access-Token", out var token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

// Configuring Cors
var FRONT_END_URL = builder.Configuration.GetValue<string>("Front-Url") ?? throw new InvalidOperationException("Front-end url is not in appsettings.json");
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(FRONT_END_URL)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Starting configuring app and http request pipeline.
var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Start app
app.Run();
