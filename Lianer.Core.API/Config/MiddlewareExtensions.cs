using Lianer.Core.API.Middleware;


// ── Middleware pipeline (correct order) ───────────────────
public static class MiddlewareExtensions
{
    private const string CorsPolicy = "DefaultPolicy";
    public static WebApplication SetupMiddleware(this WebApplication app)
    {
            // 1. Exception handling (outermost — catches everything)
            app.UseMiddleware<ExceptionMiddleware>();

            // 2. HTTPS redirection
            app.UseHttpsRedirection();

            // 3. CORS
            app.UseCors(CorsPolicy);

            // 4. Rate limiting
            app.UseRateLimiter();

            // 5. Routing
            app.UseRouting();

            // 6. Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

        return app;
    }
}