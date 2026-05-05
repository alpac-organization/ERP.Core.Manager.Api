using MediatR;
using AutoMapper;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Modules.v1.Handlers
{
    public class ObtainActiveModulesByCompanyIdHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<ObtainActiveModulesByCompanyIdQuery, List<ModuleDto>>
    {
        public async Task<List<ModuleDto>> Handle(ObtainActiveModulesByCompanyIdQuery request, CancellationToken cancellationToken)
        {

            var modules = await _unitOfWork.Modules.ObtainActiveModulesByCompanyId(request.CompanyId, cancellationToken);

            return _mapper.Map<List<ModuleDto>>(modules);
        }
    }
}