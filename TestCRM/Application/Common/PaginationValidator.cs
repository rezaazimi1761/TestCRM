using Microsoft.AspNetCore.Mvc;

namespace TestCRM.Application.Common;

/// <summary>
/// Validates page / pageSize query parameters before they reach the database.
/// Call ValidateOrBadRequest() at the top of every list action.
/// </summary>
public static class PaginationValidator
{
    public const int MaxPageSize = 100;

    /// <summary>
    /// Returns a BadRequestObjectResult if values are invalid, null otherwise.
    /// Usage:  if (PaginationValidator.ValidateOrBadRequest(page, pageSize) is { } err) return err;
    /// </summary>
    public static BadRequestObjectResult? ValidateOrBadRequest(int page, int pageSize)
    {
        if (page < 1)
            return new BadRequestObjectResult("'page' must be >= 1.");
        if (pageSize < 1 || pageSize > MaxPageSize)
            return new BadRequestObjectResult($"'pageSize' must be between 1 and {MaxPageSize}.");
        return null;
    }
}
