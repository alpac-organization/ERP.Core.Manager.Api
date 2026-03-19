using ERP.Core.Manager.Api.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Domain.Entities.Exceptions;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    public class ErrorManager : IErrorManager
    {
        public T ThrowUnauthorized<T>(string message, string type) 
            => throw new CoreException(new ErrorResponse(401, type, message));

        public T ThrowForbidden<T>(string message, string type) 
            => throw new CoreException(new ErrorResponse(403, type, message));

        public T ThrowNotFound<T>(string message, string type) 
            => throw new CoreException(new ErrorResponse(404, type, message));

        public T ThrowBadRequest<T>(string message, string type) 
            => throw new CoreException(new ErrorResponse(400, type, message));

        public T ThrowInternalError<T>(string message, string type) 
            => throw new CoreException(new ErrorResponse(500, type, message));
            

        // Implementaciones void
        public void ThrowUnauthorized(string message, string type) => ThrowUnauthorized<object>(message, type);
        public void ThrowForbidden(string message, string type) => ThrowForbidden<object>(message, type);
        public void ThrowNotFound(string message, string type) => ThrowNotFound<object>(message, type);
        public void ThrowBadRequest(string message, string type) => ThrowBadRequest<object>(message, type);
        public void ThrowInternalError(string message, string type) => ThrowInternalError<object>(message, type);

        public void ThrowValidatorsErrors(IEnumerable<string> errors, string type)
        {
            var message = string.Join(" | ", errors);    
            ThrowBadRequest(message, type);
        }
    }
}