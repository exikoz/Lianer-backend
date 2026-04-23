using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Azure.Identity;
using Lianer.Core.API.Data;
using Lianer.Core.API.Filters;
using Lianer.Core.API.Middleware;
using Lianer.Core.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

namespace Lianer.Core.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Azure Key Vault (Always active) ---
            var vaultUri = builder.Configuration["AzureKeyVault:VaultUri"];
            if (!string.IsNullOrEmpty(vaultUri))
            {
                builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
            }

            // ── Services ──────────────────────────────────────────────

            // Configure JSON serialization with camelCase for frontend compatibility
            builder.Services.AddControllers(options =>
            {
                // Register custom validation filter globally
                options.Filters.Add<ValidateModelFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            });

            // Database (EF Core InMemory)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("LianerDb"));

            // Register application services (DI)
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

            // Configure HTTP clients with Polly resilience patterns
            builder.Services.AddHttpClient("GoogleAuth", client =>
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "Lianer-Backend/1.0");
            });

            // --- Configure JWT Authentication ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

// Förhindra krasch vid tester/utveckling om nyckeln saknas
if (string.IsNullOrEmpty(secretKey))
{
    if (builder.Environment.IsProduction())
    {
        throw new InvalidOperationException("JWT SecretKey MUST be configured in Production!");
    }
    // Fallback för Development/Testing
    secretKey = "DevelopmentSecretKeyMinstTrettioTvåTeckenLång!!"; 
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
            ValidIssuer = jwtSettings["Issuer"] ?? "LianerIssuer",
            ValidAudience = jwtSettings["Audience"] ?? "LianerAudience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

            builder.Services.AddAuthorization();

            // Configure CORS — strict policy, no AllowAnyOrigin
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("DefaultPolicy", policy =>
                {
                    policy.WithOrigins(
                            builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                            ?? ["http://localhost:5173", "http://localhost:3000"])
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // Configure Rate Limiting — Fixed Window
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddFixedWindowLimiter("fixed", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 100;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 10;
                });
            });

            // OpenAPI
            builder.Services.AddOpenApi();

            // API versioning (Asp.Versioning)
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Validate DI on build to catch missing registrations early
            builder.Host.UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            });

            var app = builder.Build();

            // Ensure database is created
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            // ── Middleware pipeline (correct order) ───────────────────
            // 1. Exception handling (outermost — catches everything)
            app.UseMiddleware<ExceptionMiddleware>();

            // 2. HTTPS redirection
            app.UseHttpsRedirection();

            // 3. CORS
            app.UseCors("DefaultPolicy");

            // 4. Rate limiting
            app.UseRateLimiter();

            // 5. Routing
            app.UseRouting();

            // 6. Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // 7. OpenAPI / Scalar (dev only)
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.MapControllers()
                .RequireRateLimiting("fixed");

            app.Run();
        }
    }
}
