using Ardalis.Result;

namespace Arda9FileApi.Application.Extensions;

public static class ResultError
{
    /// <summary>
    /// Cria um Result.Error com CorrelationId do HttpContext
    /// </summary>
    public static Result Error(string errorMessage, string? correlationId = null)
    {
        var errorList = new ErrorList(
            new[] { errorMessage },
            correlationId ?? Guid.NewGuid().ToString()
        );
        
        return Result.Error(errorList);
    }

    /// <summary>
    /// Cria um Result.Error com múltiplas mensagens de erro
    /// </summary>
    public static Result Error(IEnumerable<string> errorMessages, string? correlationId = null)
    {
        var errorList = new ErrorList(
            errorMessages,
            correlationId ?? Guid.NewGuid().ToString()
        );
        
        return Result.Error(errorList);
    }

    /// <summary>
    /// Cria um Result<T>.Error com CorrelationId
    /// </summary>
    public static Result<T> Error<T>(string errorMessage, string? correlationId = null)
    {
        var errorList = new ErrorList(
            new[] { errorMessage },
            correlationId ?? Guid.NewGuid().ToString()
        );
        
        return Result<T>.Error(errorList);
    }

    /// <summary>
    /// Cria um Result<T>.Error com múltiplas mensagens de erro
    /// </summary>
    public static Result<T> Error<T>(IEnumerable<string> errorMessages, string? correlationId = null)
    {
        var errorList = new ErrorList(
            errorMessages,
            correlationId ?? Guid.NewGuid().ToString()
        );
        
        return Result<T>.Error(errorList);
    }
}