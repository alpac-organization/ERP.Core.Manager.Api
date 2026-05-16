using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries
{
    public class GetVacationControlQuery : BaseRequest, IRequest<PagedResponse<VacationAccruals>>
    {
        public VacationReportType Type { get; set; }
        public Guid? BranchId { get; set; }

        public int? WorkAreaId { get; set; }
        public string? IdentificationNumber { get; set; }

        public int PageSize { get; set; }
        public int PageNumber { get; set; }     
    }
}