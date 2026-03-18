using MediatR;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands
{
    public class CreateNewUserCommand : IRequest<CreateUserDto>
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Password { get; set; }

        [JsonIgnore]
        public int CompanyId { get; set; }
        public List<ModulesWithAccessAndRole> ModulesWithAccess { get; set; } = [];
    }

    public class ModulesWithAccessAndRole
    {
        public int ModuleId { get; set; }
        public Guid RoleId { get; set; }
    }
}