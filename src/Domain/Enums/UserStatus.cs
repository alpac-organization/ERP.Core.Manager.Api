using System.Runtime.Serialization;

namespace ERP.Core.Manager.Api.Domain.Enums
{
    /// <summary>
    /// Representa los posibles estados de una cuenta de usuario dentro del sistema.
    /// Controla la capacidad del usuario para autenticarse y realizar operaciones.
    /// </summary>
    public enum UserStatus
    {
        /// <summary>
        /// El usuario se encuentra plenamente operativo y tiene permitido 
        /// el acceso al sistema según sus credenciales.
        /// </summary>
        [EnumMember(Value = "Active")]
        Active,

        /// <summary>
        /// El usuario ha sido deshabilitado manualmente. No puede iniciar sesión 
        /// ni realizar ninguna operación, pero su información se conserva para fines históricos.
        /// </summary>
        [EnumMember(Value = "Inactive")]
        Inactive,

        /// <summary>
        /// La cuenta ha sido bloqueada automáticamente (generalmente por exceder 
        /// el número de intentos de inicio de sesión fallidos) o por razones de seguridad.
        /// Requiere intervención administrativa para ser desbloqueada.
        /// </summary>
        [EnumMember(Value = "Locked")]
        Locked
    }
}