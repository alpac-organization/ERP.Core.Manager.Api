using MediatR;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands
{
    public class LoginWithUsernameAndPasswordCommand : IRequest<LoginDto>
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }

        [JsonIgnore]
        public int CompanyId { get; set; }

        public SessionDetails? SessionDetails { get; set; }
    }

    public class SessionDetails
    {
        public string? DeviceName { get; set; }
        
        [JsonIgnore]
        public string? IpAddress { get; set; }
    }
}