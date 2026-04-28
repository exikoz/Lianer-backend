using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Lianer.Core.API.Data;
using Lianer.Core.API.Filters;
using Lianer.Core.API.Middleware;
using Lianer.Core.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
                Console.WriteLine("Core API: Key Vault connection initialized to: " + vaultUri);
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

            // --- Caching Support (K-128) ---
            builder.Services.AddMemoryCache();

            // Register application services (DI)
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<UserRepository>();

            // Activity services & repository
            builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
            builder.Services.AddScoped<IActivityService, ActivityService>();
            builder.Services.AddScoped<IActivityQueryService, ActivityQueryService>();

            // Note services & repository
            builder.Services.AddScoped<INoteRepository, NoteRepository>();
            builder.Services.AddScoped<INoteService, NoteService>();
            builder.Services.AddScoped<INoteQueryService, NoteQueryService>();

            // Contact service & repository
            builder.Services.AddScoped<IContactRepository, ContactRepository>();
            builder.Services.AddScoped<ContactService>();

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
    secretKey = "LianerBackendSharedDevelopmentSecretKey2026!!"; 
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
            ValidIssuer = jwtSettings["Issuer"] ?? "http://localhost:5297",
            ValidAudience = jwtSettings["Audience"] ?? "http://localhost:5297",
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

            // OpenAPI with Google OAuth2 security scheme
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, ct) =>
                {
                    var oauthScheme = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/v2/auth"),
                                TokenUrl = new Uri("https://oauth2.googleapis.com/token"),
                                Scopes = new Dictionary<string, string>
                                {
                                    { "openid", "OpenID information" },
                                    { "email", "Email address" },
                                    { "profile", "Profile information" }
                                }
                            }
                        }
                    };

                    document.Components ??= new();
                    document.Components.SecuritySchemes.Add("GoogleAuth", oauthScheme);

                    // Add Bearer Token (JWT) Scheme
                    var bearerScheme = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
                    };
                    document.Components.SecuritySchemes.Add("BearerAuth", bearerScheme);

                    // Add global security requirements (BearerAuth is first for default selection in Scalar)
                    document.SecurityRequirements.Add(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "BearerAuth" }
                            },
                            new List<string>()
                        }
                    });
                    document.SecurityRequirements.Add(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "GoogleAuth" }
                            },
                            new List<string>()
                        }
                    });

                    return Task.CompletedTask;
                });
            });

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
                app.MapScalarApiReference(options =>
                {
                    // Denna metod ersätter den föråldrade OAuth2Options
                    // Vi skickar med namnet "GoogleAuth" som matchar vår DocumentTransformer
                    options.AddAuthorizationCodeFlow("GoogleAuth", auth =>
                    {
                        auth.ClientId = builder.Configuration["Google:Auth:ClientId"];
                        auth.SelectedScopes = ["openid", "email", "profile"];
                    });
                });
            }

            app.MapControllers()
                .RequireRateLimiting("fixed");

            app.Run();
        }
    }
}
