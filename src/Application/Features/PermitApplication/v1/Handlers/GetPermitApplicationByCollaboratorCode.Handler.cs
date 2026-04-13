using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Queries;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Handlers
{
    public class GetPermitApplicationByCollaboratorCodeHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper): AlpacBaseHandler<GetPermitApplicationByCollaboratorCodeQuery, PermitApplicationDto>(_unitOfWork, _errorManager)
    {
        public override async Task<PermitApplicationDto> Handle(GetPermitApplicationByCollaboratorCodeQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var permit = await _unitOfWork.PermitApplications.Entities
                .Include(per => per.Collaborator)
                .Where(per => per.Collaborator.CollaboratorCode == request.CollaboratorCode)
                .Where(per => per.Collaborator.CompanyId == request.CompanyId)
                .Where(per => per.Status == PermitApplicationStatus.Pending)
                .FirstOrDefaultAsync(cancellationToken);

            return _mapper.Map<PermitApplicationDto>(permit);
        }
    }
}