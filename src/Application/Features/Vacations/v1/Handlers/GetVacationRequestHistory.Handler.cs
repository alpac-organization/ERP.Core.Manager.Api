using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Handlers
{
    public class GetVacationRequestHistoryHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper): AlpacBaseHandler<GetVacationRequestHistoryQuery, List<VacationRequestDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<List<VacationRequestDto>> Handle(GetVacationRequestHistoryQuery request, CancellationToken cancellationToken)
        {
            //Comenzar logica para mapeo de datos
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var collaborator = await _unitOfWork.Collaborators.Entities
                .Where(c => c.CompanyId == request.CompanyId)
                .Where(c => c.IdentificationNumber == request.IdentificationNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (collaborator is null)
            {
                return _errorManager.ThrowBadRequest<List<VacationRequestDto>>("Este colaborador no existe", "ERP:001");
            }

            var query = _unitOfWork.VacationRequests.Entities
                .Include(info => info.Collaborator)
                .Where(info => info.Collaborator.CompanyId == request.CompanyId)
                .Where(info => info.Collaborator.IdentificationNumber == request.IdentificationNumber)
                .AsQueryable();

            if (request.Status.HasValue)
            {
                query = query.Where(info => info.Status == request.Status.Value);
            }

            var items = await query
                .OrderByDescending(info => info.CreatedAt) 
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);            

            return _mapper.Map<List<VacationRequestDto>>(items);
        }
    }
}