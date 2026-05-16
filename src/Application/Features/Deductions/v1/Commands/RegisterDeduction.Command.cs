using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands
{
    public class RegisterDeductionCommand: BaseRequest, IRequest<bool>
    {
        //Registrar el periodo las deducciones
        public Guid PayrollId { get; set; } 
        public DeductionType DeductionType { get; set; }


        public LoansPayload? LoansPayload { get; set; }
        public AdvanceSalaryPayload? AdvanceSalaryPayload { get; set; }

        
        //Importación de documentos aqui.
        public List<PurisimaData> PurisimaData { get; set; } = [];
        public List<LateArrivalsData> LateArrivalsData { get; set; } = [];
    }

    public class PurisimaData
    {
        public decimal Amount { get; set; }
        public string? IdentificationNumber { get; set; }
    }

    public class LateArrivalsData
    {
        public decimal TotalMinutes { get; set; }
        public string? IdentificationNumber { get; set; }
    }

    public class AdvanceSalaryPayload
    {
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public Guid CollaboratorId { get; set; }
    }

    public class LoansPayload
    {
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public int NumberFortnights { get; set; }
    }
}