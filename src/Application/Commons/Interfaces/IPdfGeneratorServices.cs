namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IPdfGeneratorServices
    {
        Task<byte[]> GenerateAsync<T>(string templateName, object data);
    }

}