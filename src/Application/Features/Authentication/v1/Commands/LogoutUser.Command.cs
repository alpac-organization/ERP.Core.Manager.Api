using MediatR;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands
{
    public class LogoutUserCommand : IRequest<bool>
    {
        public string? RefreshToken { get; set; }
        
        [JsonIgnore]
        public int CompanyId { get; set; }
    }   
}