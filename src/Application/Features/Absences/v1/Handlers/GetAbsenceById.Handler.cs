using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Handlers
{
    public class GetAbsenceByIdRequestHandler(IUnitOfWork _unitOfWork, IMapper _mapper, IErrorManager _errorManager)
        : AlpacBaseHandler<GetAbsenceByIdQuery, AbsenceDetailDto?>(_unitOfWork, _errorManager)
    {
        public override async Task<AbsenceDetailDto?> Handle(GetAbsenceByIdQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);
            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }

            var record = await _unitOfWork.Absences.Entities
                .Include(a => a.Collaborator)
                    .ThenInclude(c => c.WorkingInformation)
                        .ThenInclude(w => w.Branch)
                .Where(a => a.Id == request.AbsenceId && a.Collaborator.CompanyId == request.CompanyId && a.DeletedAt == null)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (record is null)
            {
                return _errorManager.ThrowNotFound<AbsenceDetailDto?>("No se encontró el registro de inasistencia especificado.", "ERP:004");
            }

            return _mapper.Map<AbsenceDetailDto>(record);
        }
    }
}
