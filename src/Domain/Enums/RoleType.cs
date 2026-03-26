using System.Runtime.Serialization;

namespace ERP.Core.Manager.Api.Domain.Enums
{
    /// <summary>
    /// Define los tipos de roles fundamentales dentro del sistema ERP.
    /// Estos roles determinan el nivel de acceso jerárquico y las responsabilidades 
    /// globales de un usuario independientemente del módulo.
    /// </summary>
    public enum RoleType
    {
        /// <summary>
        /// Acceso total al sistema. Puede gestionar configuraciones globales, 
        /// seguridad, usuarios y auditoría de logs.
        /// </summary>
        [EnumMember(Value = "Administrator")]
        Administrator,

        /// <summary>
        /// Rol de control intermedio. Tiene capacidad para autorizar registros, 
        /// visualizar reportes gerenciales y supervisar flujos de trabajo.
        /// </summary>
        [EnumMember(Value = "Supervisor")]
        Supervisor = 3,

        /// <summary>
        /// Usuario ejecutor de tareas puntuales. Su acceso está limitado 
        /// a registros operativos específicos (ej. entradas de almacén o producción).
        /// </summary>
        [EnumMember(Value = "Operator")]
        Operator,
    }
}