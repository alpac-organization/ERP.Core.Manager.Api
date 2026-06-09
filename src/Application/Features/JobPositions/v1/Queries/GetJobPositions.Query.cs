using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Queries
{
    public class GetJobPositionsQuery : BaseRequest, IRequest<List<JobPositionDto>>
    {
    }
}