using Microsoft.OpenApi.Models;

public static class OpenApiExtensions
{
    private const string AuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenUrl = "https://oauth2.googleapis.com/token";

    public static IServiceCollection SetupOpenAPI(this IServiceCollection services)
    {
            // OpenAPI with Google OAuth2 security scheme
            services.AddOpenApi(options =>
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
                                AuthorizationUrl = new Uri(AuthUrl),
                                TokenUrl = new Uri(TokenUrl),
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

        return services;
    }
}
