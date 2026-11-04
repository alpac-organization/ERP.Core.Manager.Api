using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Queries
{
    public class GetAbsencesQuery : BaseRequest, IRequest<PagedResponse<AbsenceDto>>
    {
        public Guid? BranchId { get; set; }
        public Guid? CollaboratorId { get; set; }
        public string? IdentificationNumber { get; set; }
        public AbsenceStatus? Status { get; set; }
        public AbsenceSource? Source { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public Guid? PayrollId { get; set; }

        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }
}
