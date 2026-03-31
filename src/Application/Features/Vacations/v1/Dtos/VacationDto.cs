namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos
{
    public class VacationDto
    {
        public string? FullName { get; set; }
        public decimal AvailableVacations { get; set; }
        public decimal GeneredVacation { get; set; }
        public decimal EnjoyedVacation { get; set; }
    }
}