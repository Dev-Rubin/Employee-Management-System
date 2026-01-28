using EMS.Application;
using EMS.Infrastructure;
using EMS.Logic;
using EMS.MicroService.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Serilog Configuration
// Setup Serilog from appsettings.json
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
);
#endregion

#region Services Registration

// Add Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// HTTP Context Accessor
builder.Services.AddHttpContextAccessor();
// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// Custom services
builder.Services.AddEmsLogic(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
#endregion

#region Authentication & Authorization

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            ClockSkew = TimeSpan.Zero // optional: no extra expiration time
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ActiveUser", policy => policy.RequireClaim("IsActive", "True"));
});

#endregion

var app = builder.Build();

#region Apply Pending Migrations

// Ensure DB is up-to-date
await app.ApplyPendingMigrationsAsync();

#endregion


#region Middleware Pipeline

app.UseSwagger();
app.UseSwaggerUI();

// HTTPS redirection
app.UseHttpsRedirection();
// Routing
app.UseRouting();
// Global Exception Handling
app.UseMiddleware<GlobalExceptionMiddleware>();
// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();
// User Context Middleware (after authentication)
app.UseMiddleware<UserContextMiddleware>();
// Map Controllers
app.MapControllers();

#endregion

app.Run();
