using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Domain.Enums
{
    /// <summary>
    /// Represents the types of identification documents supported by the system.
    /// </summary>
    public enum IdentificationType
    {
        NationalId = 1,
        Passport = 2,
        ResidencyCard = 3
    }
}