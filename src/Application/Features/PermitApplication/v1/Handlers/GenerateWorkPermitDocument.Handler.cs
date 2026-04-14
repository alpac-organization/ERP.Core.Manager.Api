using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Handlers
{
    public class GenerateWorkPermitDocumentHandler(
        IUnitOfWork _unitOfWork,  
        IErrorManager _errorManager,
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
            };

            string processedHtml = templateServices.Render("PermitApplication", templateModel);


    
            return new (){ DocumentUrl = $"/documents/permits" };
        }
    }
}