using MediatR;
using FluentValidation;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>(
        IEnumerable<IValidator<TRequest>> _validators,
        IErrorManager _errorManager) // <--- Inyectamos tu ErrorManager
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken))
                );

                // Extraemos todos los mensajes de error
                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .Select(f => f.ErrorMessage) // Solo el mensaje string
                    .ToList();

                if (failures.Count != 0)
                {
                    // Usamos tu método personalizado para formatear y lanzar la excepción
                    _errorManager.ThrowValidatorsErrors(failures, "Validation_Error");
                }
            }

            return await next();
        }
    }
}