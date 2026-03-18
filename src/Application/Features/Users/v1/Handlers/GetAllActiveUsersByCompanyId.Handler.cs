using MediatR;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Queries;
using ERP.Core.Manager.Api.Domain.Interfaces;
using AutoMapper;

namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Handlers
{
    public class GetAllActiveUsersByCompanyIdHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetAllActiveUsersByCompanyIdQuery, List<UserDto>>
    {
        public async Task<List<UserDto>> Handle(GetAllActiveUsersByCompanyIdQuery request, CancellationToken cancellationToken)
        {
            var usersActive = await _unitOfWork.Users.GetActiveUsersByCompany(request.CompanyId, cancellationToken);
            return _mapper.Map<List<UserDto>>(usersActive);
        }
    }
}