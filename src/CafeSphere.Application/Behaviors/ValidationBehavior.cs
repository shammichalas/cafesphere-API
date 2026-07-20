using CafeSphere.Shared.Models;
using FluentValidation;
using MediatR;

namespace CafeSphere.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .Select(f => new Error(f.ErrorCode ?? "Validation.Error", f.ErrorMessage, f.PropertyName))
            .ToList();

        if (failures.Count != 0)
        {
            // If TResponse is Result or Result<T>
            var responseType = typeof(TResponse);
            if (responseType == typeof(Result))
            {
                return (TResponse)(object)Result.ValidationFailure(failures);
            }

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = responseType.GetMethod(nameof(Result<object>.ValidationFailure));
                if (failureMethod != null)
                {
                    return (TResponse)failureMethod.Invoke(null, new object[] { failures })!;
                }
            }

            throw new Shared.Exceptions.ValidationException(failures);
        }

        return await next();
    }
}
