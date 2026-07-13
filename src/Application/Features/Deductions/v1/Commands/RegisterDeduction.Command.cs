using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands
{
    public class RegisterDeductionCommand : BaseRequest, IRequest<bool>
    {
        //Registrar el periodo las deducciones
        public Guid PayrollId { get; set; }
        public DeductionType DeductionType { get; set; }

        //✅Metodos manuales de registro de deducciones. 
        public LoansPayload? LoansPayload { get; set; }
        public OtherDeductionsData? OtherDeductionsPayload { get; set; }
        public SansionPayload? SansionPayload { get; set; }
        public JudicialSeizurePayload? judicialSeizurePayload { get; set; }

        //✅Importación de documentos aqui only(purisima, llegadas tardes).
        public PurisimaInformation PurisimaInformation { get; set; } = new();
        public LateArrivalsInformation LateArrivalsInformation { get; set; } = new();
    }

    #region Cargas utiles para registro de purisima.
    //✅Carga util para registrar purisima. (importación, manual)
    public class PurisimaInformation
    {
        public PurisimaData PurisimaPayload { get; set; } = new();
        public List<PurisimaData> PurisimaData { get; set; } = [];

        public ProcedureMethod ProcedureMethod { get; set; }
    }

    //✅Carga util para registrar deducción purisima. (importación, manual)
    public class PurisimaData
    {
        public decimal Amount { get; set; }
        public int NumberFortnights { get; set; }
        public string? IdentificationNumber { get; set; }
    }
    #endregion

    #region Cargas utiles para registro de llegadas tardes.
    //✅Carga util para registrar llegadas tardes. (importación, manual)
    public class LateArrivalsInformation
    {
        public LateArrivalsData LateArrivalsPayload { get; set; } = new();
        public List<LateArrivalsData> LateArrivalsData { get; set; } = [];

        public ProcedureMethod ProcedureMethod { get; set; }
    }

    public class LateArrivalsData
    {
        public decimal TotalMinutes { get; set; }
        public string? IdentificationNumber { get; set; }
    }
    #endregion

    //✅Carga util para registrar otras deducciones.
    public class OtherDeductionsData
    {
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public int NumberFortnights { get; set; }
        public string? Description { get; set; }
        public string? IdentificationNumber { get; set; }
    }

    //✅Carga util para registrar prestamos.
    public class LoansPayload
    {
        public Currency Currency { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public int NumberFortnights { get; set; }
        public string? IdentificationNumber { get; set; }
    }

    //✅ Carga util para registrar embargos judiciales.
    public class JudicialSeizurePayload
    {
        public Currency Currency { get; set; }
        public decimal TotalAmountToPay { get; set; } // Monto total de la deuda
        public int DeductionPercentage { get; set; }
        public string? Description { get; set; }
        public string? IdentificationNumber { get; set; }
    }

    public class SansionPayload
    {
        public int AmountDays { get; set; }
        public string? IdentificationNumber { get; set; }
    }
}