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
        Cedula = 1,

        Pasaporte = 2,

        CedulaResidencia = 3
    }
}