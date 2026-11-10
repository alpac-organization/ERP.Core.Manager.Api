using MediatR;
using System.Text.Json.Serialization;

using ERP.Core.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands
{
    public class DeleteProductCommand : BaseRequest, IRequest<Unit>
    {
        [JsonIgnore]
        public Guid ProductId { get; set; }
    }
}