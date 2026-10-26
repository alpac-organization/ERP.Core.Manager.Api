using MediatR;
using ERP.Core.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands
{
    public class UpdateDeductionCommand: BaseRequest, IRequest<bool>
    {
        
    }
}   