using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos
{
    public class DeductionDto
    {
        public DeductionType Type { get; set; }
        public string? Description { get; set; }
        public Guid CollaboratorId { get; set;}
    }

    public record PagedResponseDeduction<T>(
        List<T> Data, 
        int PageNumber,
        int PageSize,

        int TotalDeductions = 0,
        int TotalDeductionCollaborator = 0
    );
}