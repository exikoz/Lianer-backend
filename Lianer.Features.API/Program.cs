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

            // --- Database Setup ---
            builder.Services.AddDbContext<FeaturesDbContext>(options =>
                options.UseInMemoryDatabase("LianerFeaturesDb"));

            // --- Hunter.io Client & Service (Epic 6 & VG Resilience) ---
            builder.Services.AddHttpClient<HunterClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.hunter.io/");
            })
            .AddStandardResilienceHandler();

            builder.Services.AddScoped<IHunterService, HunterService>();

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
            builder.Services.AddOpenApi();

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

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
