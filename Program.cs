using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Extensions.Logging;
using ReacodeApp.Data;
using ReacodeApp.Services;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using ReacodeApp;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure MVC controllers - exclude API controllers from MVC routing
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    })
    .ConfigureApplicationPartManager(manager =>
    {
        // Remove the default ControllerFeatureProvider and add a custom one
        // that excludes API controllers from MVC routing
        var controllerFeatureProvider = manager.FeatureProviders
            .OfType<ControllerFeatureProvider>()
            .FirstOrDefault();
        
        if (controllerFeatureProvider != null)
        {
            manager.FeatureProviders.Remove(controllerFeatureProvider);
        }
        
        // Add custom feature provider that excludes API controllers
        manager.FeatureProviders.Add(new ExcludeApiControllersFeatureProvider());
    });


// Add Entity Framework with logging
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    
    // Enable EF Core logging in Development
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information)
               .EnableSensitiveDataLogging()
               .EnableDetailedErrors();
    }
});

// Add custom services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISessionService, SessionService>();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ReacodeApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ReacodeAppUsers";

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ReacodeApp API",
        Version = "v1",
        Description = "Web API for ReacodeApp - Supports both web and mobile applications"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS for mobile app (Kotlin/Android) and web app
builder.Services.AddCors(options =>
{
    // Production policy - allows mobile app from any origin (mobile apps don't have same-origin restrictions)
    // For web apps, you can restrict to specific domains
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy.AllowAnyOrigin()  // Mobile apps can come from any origin
              .AllowAnyMethod()
              .AllowAnyHeader();
        // Note: AllowCredentials() cannot be used with AllowAnyOrigin()
        // For mobile apps, this is fine as they use Bearer tokens, not cookies
    });
    
    // Development policy - allows all for local testing
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Configure HTTPS redirection
// Skip in development when running HTTP-only to avoid warnings
// Always enable in production for security
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
else
{
    // In development, only use HTTPS redirection if HTTPS is configured
    var urls = builder.Configuration["ASPNETCORE_URLS"] ?? "";
    if (urls.IndexOf("https", StringComparison.OrdinalIgnoreCase) >= 0)
    {
        app.UseHttpsRedirection();
    }
}

app.UseStaticFiles();

app.UseRouting();

// Add CORS (must be after UseRouting, before UseAuthentication and UseAuthorization)
// Mobile apps can use either policy - they don't have CORS restrictions like browsers
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("AllowMobileApp");  // Allows mobile app from any origin
}

// Add session middleware (must be after UseRouting)
app.UseSession();

// Add Authentication & Authorization (must be after UseRouting, before MapControllers)
app.UseAuthentication();
app.UseAuthorization();

// Configure Swagger - Enable in production for API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReacodeApp API v1");
    c.RoutePrefix = "swagger";
    // Optional: Disable Swagger UI in production by uncommenting below
    // if (!app.Environment.IsDevelopment()) { c.RoutePrefix = string.Empty; }
});

// Map MVC Controllers FIRST (default route pattern)
// This ensures MVC routes are available for URL generation
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map API Controllers (only controllers with [ApiController] and [Route("api/...")])
// These will only match paths starting with /api/
app.MapControllers();

app.Run();
