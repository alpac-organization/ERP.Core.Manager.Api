using MediatR;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Commands
{
    public class VerifyAccessCommand : IRequest<VerifyAccessDto>
    {
        [JsonIgnore]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public int CompanyId { get; set; }
        
        public string? ModuleCode { get; set; }
    }
}