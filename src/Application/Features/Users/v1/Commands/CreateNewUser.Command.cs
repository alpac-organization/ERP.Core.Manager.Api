using MediatR;
using ERP.Core.Database.Domain.Enums;
using System.Text.Json.Serialization;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Commands
{
    public class CreateNewUserCommand : IRequest<CreateUserDto>
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Password { get; set; }
        public UserType UserType { get; set; }
        public string? IdentificationNumber { get; set; }
        public Guid BranchId { get; set; }
        public Guid AreaId { get; set; }


        [JsonIgnore]
        public Guid CompanyId { get; set; }
        public List<ModulesWithAccessAndRole> ModulesWithAccess { get; set; } = [];
    }

    public class ModulesWithAccessAndRole
    {
        public Guid RoleId { get; set; }
        public string? ModuleCode { get; set; }
    }
}