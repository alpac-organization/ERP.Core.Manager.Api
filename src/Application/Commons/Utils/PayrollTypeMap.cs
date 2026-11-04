using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Commons.Utils
{
    public static class PayrollTypeMap
    {
        public static PayrollType? FromSalaryType(SalaryType? salaryType) => salaryType switch
        {
            SalaryType.Fixed => PayrollType.Ordinary,
            SalaryType.ProfessionalServices => PayrollType.ProfessionalServices,
            _ => null
        };

        public static SalaryType ToSalaryType(PayrollType payrollType) => payrollType switch
        {
            PayrollType.Ordinary => SalaryType.Fixed,
            PayrollType.ProfessionalServices => SalaryType.ProfessionalServices,
            _ => throw new ArgumentOutOfRangeException(nameof(payrollType), payrollType, "Tipo de nómina sin salario asociado")
        };
    }
}
