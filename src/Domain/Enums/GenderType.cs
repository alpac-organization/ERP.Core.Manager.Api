using System.Runtime.Serialization;

namespace ERP.Core.Manager.Api.Domain.Enums
{
    public enum GenderType
    {
        [EnumMember(Value = "Man")]
        Man = 1,

        [EnumMember(Value = "Women")]
        Women = 2
    }
}   