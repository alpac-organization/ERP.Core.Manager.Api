namespace ERP.Core.Manager.Api.Domain.Enums
{
    /// <summary>
    /// Define los diferentes tipos de catálogos configurables en el sistema.
    /// Estos tipos se utilizan para categorizar información maestra que estructura 
    /// la parte organizacional y operativa de una empresa.
    /// </summary>
    public enum CatalogType
    {
        /// <summary>
        /// Catálogo de sucursales de la empresa.
        /// Cada entrada representa una ubicación física o punto de venta.
        /// </summary>
        Branches = 1,

        /// <summary>
        /// Catálogo de departamentos, áreas o unidades de negocio.
        /// Define la división interna para la agrupación lógica de procesos.
        /// </summary>
        WorkAreas = 2,

        /// <summary>
        /// Catálogo de cargos o puestos de trabajo.
        /// Define las funciones y responsabilidades del organigrama corporativo.
        /// </summary>
        JobPositions = 3,

        /// <summary>
        /// Catálogo de tipos de documentos legales y comerciales.
        /// Clasifica formatos como Cédulas, RUC, Facturas, Notas de Crédito, etc.
        /// </summary>
        DocumentTypes = 4,

        /// <summary>
        /// Catálogo de entidades bancarias.
        /// Registra los bancos nacionales e internacionales donde la empresa 
        /// posee cuentas para conciliación bancaria y pagos.
        /// </summary>
        Banks = 5,

        /// <summary>
        /// Catálogo de historial de Tasas de Cambio.
        /// Almacena el valor diario de las monedas (ej. NIO vs USD) para el 
        /// cálculo automático de diferencial cambiario y transacciones bimonetarias.
        /// </summary>
        ExchangeRates = 6
    }
}