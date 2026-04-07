namespace ERP.Core.Manager.Api.Domain.Enums
{
    public enum PermitApplicationType
    {
        /// <summary>
        /// Vacaciones anuales pagadas.
        /// </summary>
        Vacation = 1,

        /// <summary>
        /// Permiso para citas o consultas médicas.
        /// </summary>
        MedicalAppointment = 2,

        /// <summary>
        /// Tiempo compensado (por horas extras trabajadas previamente).
        /// </summary>
        CompensatoryTime = 3,

                /// <summary>
        /// Permiso con goce de salario (Paid Leave).
        /// </summary>
        PaidLeave = 4,

        /// <summary>
        /// Permiso sin goce de salario (Unpaid Leave).
        /// </summary>
        UnpaidLeave = 5,

        /// <summary>
        /// Opcional: Para licencias por duelo, paternidad, etc.
        /// </summary>
        SpecialLeave = 6
    }
}