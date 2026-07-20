using System.Text.Json.Serialization;

namespace CafeSphere.Shared.Models;

public class Error
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Target { get; set; }

    public Error() { }

    public Error(string code, string message, string? target = null)
    {
        Code = code;
        Message = message;
        Target = target;
    }

    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");
}

public class Result
{
    public bool IsSuccess { get; set; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; set; } = Error.None;
    public List<Error> ValidationErrors { get; set; } = new();

    protected Result(bool isSuccess, Error error, List<Error>? validationErrors = null)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Success result cannot contain an error.");

        if (!isSuccess && error == Error.None && (validationErrors == null || validationErrors.Count == 0))
            throw new InvalidOperationException("Failure result must contain an error.");

        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors ?? new List<Error>();
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(string code, string message) => new(false, new Error(code, message));
    public static Result ValidationFailure(List<Error> errors) => new(false, new Error("Validation.Failed", "One or more validation errors occurred."), errors);
}

public class Result<T> : Result
{
    private readonly T? _value;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    public T? ValueOrDefault => _value;

    protected Result(T? value, bool isSuccess, Error error, List<Error>? validationErrors = null)
        : base(isSuccess, error, validationErrors)
    {
        _value = value;
    }

    public static Result<T> Success(T value) => new(value, true, Error.None);
    public static new Result<T> Failure(Error error) => new(default, false, error);
    public static new Result<T> Failure(string code, string message) => new(default, false, new Error(code, message));
    public static new Result<T> ValidationFailure(List<Error> errors) => new(default, false, new Error("Validation.Failed", "One or more validation errors occurred."), errors);

    public static implicit operator Result<T>(T value) => Success(value);
}
