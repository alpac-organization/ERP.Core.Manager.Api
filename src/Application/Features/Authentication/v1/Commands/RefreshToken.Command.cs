using MediatR;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands
{
    public class RefreshTokenCommand : IRequest<LoginDto>
    {
        public string? RefreshToken { get; set; }
        
        [JsonIgnore]
        public Guid CompanyId { get; set; }
    }   
}