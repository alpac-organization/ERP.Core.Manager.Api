using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos
{
    public class DeductionDto
    {
        public Guid DeductionId { get; set; }
        public DeductionType Type { get; set; }
        public DeductionStatus Status { get; set; }

        public string? CollaboratoFullname { get; set; }
        public string? IdentificationNumber { get; set; }
    }

    public record PagedResponseDeduction<T>(
        List<T> Data, 
        
        int PageNumber,
        
        int PageSize,

        int TotalDeductions = 0
    );
}