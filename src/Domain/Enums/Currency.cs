using System.Runtime.Serialization;

namespace ERP.Core.Manager.Api.Domain.Enums
{
    public enum Currency
    {
        [EnumMember(Value = "NIO")]
        NIO = 1,

        [EnumMember(Value = "USD")]
        USD = 2
    }
}