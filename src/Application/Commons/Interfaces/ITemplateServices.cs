namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface ITemplateServices
    {
        public string Render(string templateName, object model);
    }
}