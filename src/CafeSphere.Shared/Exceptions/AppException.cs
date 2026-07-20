using CafeSphere.Shared.Models;

namespace CafeSphere.Shared.Exceptions;

public class AppException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }
    public List<Error>? ValidationErrors { get; }

    public AppException(string message, string code = "Application.Error", int statusCode = 500, List<Error>? validationErrors = null)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
        ValidationErrors = validationErrors;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with key '{key}' was not found.", "Resource.NotFound", 404)
    {
    }

    public NotFoundException(string message)
        : base(message, "Resource.NotFound", 404)
    {
    }
}

public class ValidationException : AppException
{
    public ValidationException(List<Error> errors)
        : base("One or more validation failures occurred.", "Validation.Failure", 400, errors)
    {
    }

    public ValidationException(string message)
        : base(message, "Validation.Failure", 400)
    {
    }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized access.")
        : base(message, "Auth.Unauthorized", 401)
    {
    }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Access forbidden for this operation.")
        : base(message, "Auth.Forbidden", 403)
    {
    }
}

public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message, "Resource.Conflict", 409)
    {
    }
}
