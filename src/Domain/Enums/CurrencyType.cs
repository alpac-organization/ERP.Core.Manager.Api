namespace ERP.Core.Manager.Api.Domain.Enums
{
    /// <summary>
    /// Define las monedas oficiales soportadas por el sistema ERP.
    /// Utilizado para la multimoneda en transacciones, presupuestos y reportes financieros.
    /// </summary>
    public enum CurrencyType
    {
        /// <summary>
        /// Córdoba (Nicaragua). 
        /// Moneda de curso legal y moneda principal para registros contables locales.
        /// </summary>
        NIO = 1,

        /// <summary>
        /// Dólar Estadounidense.
        /// Moneda de referencia para transacciones internacionales y ajustes por diferencial cambiario.
        /// </summary>
        USD = 2
    }
}