using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Api.ExceptionHandling;

public sealed class DomainExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not (BusinessRuleValidationException or ArgumentException or FormatException)) return false;
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails { Status = 400, Title = "Domain validation failed", Detail = exception.Message, Type = "https://httpstatuses.com/400" }
        });
    }
}
