namespace ERP.Core.Manager.Api.Domain.Enums
{
    public enum MaritalStatus
    {
        /// <summary>
        /// No definido
        /// </summary>
        None = 0,

        /// <summary>
        /// Persona que no ha contraído matrimonio.
        /// </summary>
        Single = 1,

        /// <summary>
        /// Persona que ha contraído matrimonio legalmente.
        /// </summary>
        Married = 2,

        /// <summary>
        /// Persona cuyo matrimonio ha sido disuelto legalmente.
        /// </summary>
        Divorced = 3,

        /// <summary>
        /// Persona cuyo cónyuge ha fallecido.
        /// </summary>
        Widowed = 4,

        /// <summary>
        /// Pareja que convive sin estar casada (dependiendo de la legislación local).
        /// </summary>
        DomesticPartner = 5,

        /// <summary>
        /// Persona casada que ya no convive con su cónyuge pero no se ha divorciado.
        /// </summary>
        Separated = 6,

        /// <summary>
        /// Opción para casos donde el usuario prefiere no revelar la información.
        /// </summary>
        Other = 7
    }
}