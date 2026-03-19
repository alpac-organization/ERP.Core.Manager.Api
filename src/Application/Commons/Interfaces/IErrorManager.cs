namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IErrorManager
    {
        T ThrowUnauthorized<T>(string message, string type);
        T ThrowForbidden<T>(string message, string type);
        T ThrowNotFound<T>(string message, string type);
        T ThrowBadRequest<T>(string message, string type);
        T ThrowInternalError<T>(string message, string type);


        // Versiones void (Síncronas)
        void ThrowForbidden(string message, string type);
        void ThrowUnauthorized(string message, string type);
        void ThrowNotFound(string message, string type);
        void ThrowBadRequest(string message, string type);
        void ThrowInternalError(string message, string type);
        void ThrowValidatorsErrors(IEnumerable<string> errors, string type);
    }
}