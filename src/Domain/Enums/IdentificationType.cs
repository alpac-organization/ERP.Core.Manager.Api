using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Domain.Enums
{
    /// <summary>
    /// Represents the types of identification documents supported by the system.
    /// </summary>
    public enum IdentificationType
    {
        [EnumMember(Value = "Cedula")]
        Cedula = 1,

        [EnumMember(Value = "Pasaporte")]
        Pasaporte = 2,

        [EnumMember(Value = "CedulaResidencia")]
        CedulaResidencia = 3
    }
}