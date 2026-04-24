using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.OpenApi;
using Azure.Identity;
using Lianer.Features.API.Clients;
using Lianer.Features.API.Services;
using Asp.Versioning;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Lianer.Features.API.Data;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;

namespace Lianer.Features.API
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

            // --- Caching Support (K-128) ---
            builder.Services.AddMemoryCache();

            // --- Database Setup ---
            builder.Services.AddDbContext<FeaturesDbContext>(options =>
                options.UseInMemoryDatabase("LianerFeaturesDb"));

            // --- Hunter.io Client & Service (Epic 6 & VG Resilience) ---
            builder.Services.AddHttpClient<HunterClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.hunter.io/");
            })
            .AddStandardResilienceHandler(options =>
            {
                // Ensure it handles HttpRequestException and non-success status codes
                options.Retry.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r => !r.IsSuccessStatusCode);

                options.Retry.OnRetry = args =>
                {
                    Console.WriteLine($"[Polly: Retry] HunterClient attempt {args.AttemptNumber} due to: {args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString()}");
                    return default;
                };
            });

            // --- Core API Typed Client ---
            // Configures a custom resilience pipeline for the Core API client.
            // Includes exponential retry and a circuit breaker to prevent cascading failures.
            builder.Services.AddHttpClient<CoreApiClient>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5297/");
            })
            .AddResilienceHandler("core-api-pipeline", pipeline =>
            {
                pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    Delay = TimeSpan.FromSeconds(2),
                    OnRetry = args =>
                    {
                        Console.WriteLine($"[Polly: Retry] Attempt {args.AttemptNumber} for CoreApiClient due to: {args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString()}");
                        return default;
                    }
                });

                pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 2,
                    BreakDuration = TimeSpan.FromSeconds(60),
                    OnOpened = args =>
                    {
                        Console.WriteLine("[Polly: Circuit Breaker] Opened for 60 seconds. Core API is likely down.");
                        return default;
                    },
                    OnClosed = args =>
                    {
                        Console.WriteLine("[Polly: Circuit Breaker] Closed. Core API is back online.");
                        return default;
                    },
                    OnHalfOpened = args =>
                    {
                        Console.WriteLine("[Polly: Circuit Breaker] Half-Opened. Testing Core API health...");
                        return default;
                    }
                });
            });

            builder.Services.AddScoped<IHunterService, HunterService>();

            // --- Configure JWT Authentication ---
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            // Fallback for development if secret key is missing in Key Vault/Secrets
            if (string.IsNullOrEmpty(secretKey))
            {
                if (builder.Environment.IsProduction())
                {
                    throw new InvalidOperationException("JWT SecretKey MUST be configured in Production!");
                }
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

            builder.Services.AddControllers();

            // --- API Versioning ---
            builder.Services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Components ??= new();
                    
                    var bearerScheme = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
                    };
                    document.Components.SecuritySchemes.Add("BearerAuth", bearerScheme);

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

                    return Task.CompletedTask;
                });
            });

            var app = builder.Build();

            // --- Ensure Database is Created ---
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FeaturesDbContext>();
                dbContext.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
