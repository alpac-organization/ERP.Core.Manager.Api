using MediatR;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Queries
{
    public class GetAvailableModulesForUserQuery : IRequest<List<UserModuleDto>> 
    {
        public Guid CompanyId { get;  set; }
        public Guid UserId { get; set; }
    }
}