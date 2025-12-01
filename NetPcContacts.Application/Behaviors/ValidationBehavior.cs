using FluentValidation;
using Mediator;

namespace NetPcContacts.Application.Behaviors;

/// <summary>
/// Pipeline behavior do walidacji requestów Mediator za pomocą FluentValidation.
/// Wykonuje walidację przed przekazaniem requestu do handlera.
/// </summary>
/// <typeparam name="TRequest">Typ requestu (command/query)</typeparam>
/// <typeparam name="TResponse">Typ odpowiedzi</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(message, cancellationToken);
        }

        var context = new ValidationContext<TRequest>(message);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next(message, cancellationToken);
    }
}
