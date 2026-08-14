using BookingPlatform.Application.Interfaces;
using BookingPlatform.Infrastructure.Auth;
using BookingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Scalar.AspNetCore;
using BookingPlatform.Api.Middleware;
using BookingPlatform.Infrastructure.Services;
using BookingPlatform.Infrastructure.Scheduling;
using BookingPlatform.Infrastructure.Catalog;
using BookingPlatform.Infrastructure.Bookings;
using BookingPlatform.Infrastructure.Reviews;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProviderService, ProviderService>();
builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
builder.Services.AddScoped<ISlotService, SlotService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// --- Middleware pipeline, order matters below ---

app.UseMiddleware<ExceptionHandlingMiddleware>();   // 1. catch everything first

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.EnableDarkMode();
});

                           // 3. force HTTPS early
app.UseCors("AllowAll");                               // 3.5 allow cross-origin requests
app.UseAuthentication();                              // 4. who is calling?
app.UseAuthorization();                               // 5. what are they allowed to do?

app.MapControllers();                                 // 6. finally, handle the request

try
{
    Log.Information("Starting BookingPlatform API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}