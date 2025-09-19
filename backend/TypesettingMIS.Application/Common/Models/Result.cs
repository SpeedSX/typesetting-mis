namespace TypesettingMIS.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public string[]? ValidationErrors { get; init; }

    public static Result<T> Success(T data) => new() 
    { 
        IsSuccess = true, 
        Data = data 
    };

    public static Result<T> Failure(string errorMessage) => new() 
    { 
        IsSuccess = false, 
        ErrorMessage = errorMessage 
    };

    public static Result<T> ValidationFailure(string[] errors) => new() 
    { 
        IsSuccess = false, 
        ValidationErrors = errors,
        ErrorMessage = "Validation failed"
    };
}

public class Result
{
    public bool IsSuccess { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public string[]? ValidationErrors { get; init; }

    public static Result Success() => new() { IsSuccess = true };

    public static Result Failure(string errorMessage) => new() 
    { 
        IsSuccess = false, 
        ErrorMessage = errorMessage 
    };

    public static Result ValidationFailure(string[] errors) => new() 
    { 
        IsSuccess = false, 
        ValidationErrors = errors,
        ErrorMessage = "Validation failed"
    };
}