using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lianer.Core.API.Filters;

/// <summary>
/// Custom action filter that centralizes model validation.
/// Automatically returns 400 Bad Request with validation errors
/// if ModelState is invalid, removing the need for manual checks in controllers.
/// </summary>
public class ValidateModelFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(
                new ValidationProblemDetails(context.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Failed",
                    Instance = context.HttpContext.Request.Path
                });
        }
    }
}
