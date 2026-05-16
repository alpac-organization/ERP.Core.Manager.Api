namespace ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Dtos
{
    public class TypesIncomeDto
    {
        public Guid TypeIncomeId { get; set; }
        public string? IncomeTitle { get; set; }
        public string? IncomeDescription { get; set; }
        public string? IncomeCode { get; set; }
    }
}