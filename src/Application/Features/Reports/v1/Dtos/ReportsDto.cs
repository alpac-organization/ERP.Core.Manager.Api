namespace ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos
{
    public class ReportsDto
    {
        public List<AssignedTravelExpensesHistory> AssignedTravelExpensesHistories { get; set; } = [];
    }

    public class AssignedTravelExpensesHistory
    {
        public Guid PayrollId { get; set; }
        public Guid CollaboratorId { get; set; }
        
        public int NumberDaysPaid { get; set; } //Cantidad de dias pagados. esto se calcula en base a los dias que tuvo asistencia el colaborador

        public decimal Feeding { get; set; }
        public decimal Lodging { get; set; }
        public decimal Transport { get; set; }
        public decimal TotalAmountPaid { get; set; }
    }
}