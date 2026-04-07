namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IPdfServices
    {
        public byte[] GeneratePdfFromHtml(string htmlContent);
    }
}