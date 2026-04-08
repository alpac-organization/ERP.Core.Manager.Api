using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Queries;
using ERP.Core.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Handlers
{
    public class GenerateWorkPermitDocumentHandler(
        IUnitOfWork _unitOfWork,  
        IErrorManager _errorManager,
        IPdfServices _pdfServices,
        ITemplateServices templateServices
    
    ) : AlpacBaseHandler<GenerateWorkPermitDocumentQuery, PermitApplicationDocumentDto>(_unitOfWork, _errorManager)
    {
        public override async Task<PermitApplicationDocumentDto> Handle(GenerateWorkPermitDocumentQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var permitApplicationData = await _unitOfWork.PermitApplications.Entities
                .Where(per => per.Id == request.PermitApplicationRequestId)
                .FirstOrDefaultAsync(cancellationToken);

            if(permitApplicationData is null)
            {
                return _errorManager.ThrowBadRequest<PermitApplicationDocumentDto>("Esta solicitud no fue encontrada!", "ERP:01");
            }

            var company = await _unitOfWork.Companies
                .FirstOrDefaultAsync(company => company.Id == request.CompanyId, cancellationToken);

            var templateModel = new {
                company_name = company?.CompanieName ?? "",
                logo_url = company?.ImageUrl ?? "", 
                description = permitApplicationData.Description
                // area_name = "RECURSOS HUMANOS", // O permitData.Employee.Area.Name
                // current_date = DateTime.Now.ToString("dd/MM/yyyy"),
                // collaborator_fullname = $"{permitData.Employee?.FirstName} {permitData.Employee?.LastName}",
                
                // // Fechas de Inicio y Fin
                // start_date_formatted = permitData.StartDate.ToString("dd/MM/yyyy hh:mm tt"),
                // end_date_formatted = permitData.EndDate.ToString("dd/MM/yyyy hh:mm tt"),
                
                // // Totales
                // amount_days = permitData.TotalDays,
                // amount_hours = permitData.TotalHours,
                // description = permitData.Reason,

                // // Lógica de Checks (Basado en el Tipo de Permiso)
                // check_vacation = permitData.TypeId == 1 ? "X" : "",
                // check_medical = permitData.TypeId == 2 ? "X" : "",
                // check_comp = permitData.TypeId == 3 ? "X" : "",
                // check_paid = permitData.TypeId == 4 ? "X" : "",
                // check_unpaid = permitData.TypeId == 5 ? "X" : "",
                // check_special = permitData.TypeId == 6 ? "X" : ""
            };

            string processedHtml = templateServices.Render("PermitApplication", templateModel);

            byte[] pdfBytes = _pdfServices.GeneratePdfFromHtml(processedHtml);

            string fileName = $"Permiso_{company?.Alias ?? ""}_{DateTime.Now.Ticks}.pdf";
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp_docs");
    
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fullPath = Path.Combine(folderPath, fileName);
            await File.WriteAllBytesAsync(fullPath, pdfBytes, cancellationToken);
            
            return new (){ DocumentUrl = $"/documents/permits/{fileName}" };
        }
    }
}