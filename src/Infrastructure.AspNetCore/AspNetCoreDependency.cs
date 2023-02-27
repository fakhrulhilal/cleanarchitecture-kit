using DevKit.Application.Ports;
using DevKit.Domain.Exceptions;
using DevKit.Infrastructure.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class AspNetCoreDependency
{
    public static IServiceCollection AddAspNetCore(this IServiceCollection services,
        string baseApiPath = "/api") => services
        .AddHttpContextAccessor()
        .AddSingleton<ICurrentUserService, CurrentUserService>()
        .AddProblemDetails(opt =>
        {
            var handledExceptions = new[] {
                typeof(ValidationException),
                typeof(NotFoundException),
                typeof(UnauthenticatedException),
                typeof(ForbiddenAccessException)
            };
            opt.Rethrow<StackOverflowException>();
            opt.Rethrow<OutOfMemoryException>();
            opt.Map<ValidationException>(validation =>
                new ValidationProblemDetails(validation.Errors) {
                    Title = "Bad request", Status = StatusCodes.Status400BadRequest
                });
            opt.Map<NotFoundException>(_ =>
                new StatusCodeProblemDetails(StatusCodes.Status404NotFound) {
                    Instance = $"{baseApiPath}/{_.Entity}"
                });
            bool isDevelopment =
                (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                 Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty) == "Development";
            opt.IncludeExceptionDetails = (_, exc) =>
                isDevelopment && handledExceptions.Any(t => t.IsInstanceOfType(exc));
            opt.MapToStatusCode<UnauthenticatedException>(StatusCodes.Status401Unauthorized);
            opt.MapToStatusCode<ForbiddenAccessException>(StatusCodes.Status403Forbidden);
            opt.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
        })
        .Configure<ApiBehaviorOptions>(opt => opt.InvalidModelStateResponseFactory = http =>
            new BadRequestObjectResult(new ValidationProblemDetails(http.ModelState) {
                Instance = http.HttpContext.Request.Path,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://httpstatuses.com/400",
                Detail = "One or more validation failures have occurred."
            }));
}
