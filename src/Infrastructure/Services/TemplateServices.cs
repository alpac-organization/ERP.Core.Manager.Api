using HandlebarsDotNet;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    public class TemplateServices : ITemplateServices
    {
        public string Render(string templateName, object model)
        {
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", $"{templateName}.html");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"No se encontró la plantilla de diseño en: {templatePath}");
            }

            string source = File.ReadAllText(templatePath);

            var template = Handlebars.Compile(source);

            return template(model);
        }
    }
}