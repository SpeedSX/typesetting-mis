using FluentValidation;
using MediatR;
using TypesettingMIS.Application.Common.Models;

namespace TypesettingMIS.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            var errors = failures.Select(f => f.ErrorMessage).ToArray();
            
            // Check if TResponse is a Result type
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(TResponse).GetGenericArguments()[0];
                var validationFailureMethod = typeof(Result<>).MakeGenericType(resultType)
                    .GetMethod(nameof(Result<object>.ValidationFailure), [typeof(string[])]);
                
                var result = validationFailureMethod!.Invoke(null, [errors]);
                return (TResponse)result!;
            }
            
            if (typeof(TResponse) == typeof(Result))
            {
                var result = Result.ValidationFailure(errors);
                return (TResponse)(object)result;
            }

            // For non-Result types, throw validation exception
            throw new ValidationException(failures);
        }

        return await next();
    }
}