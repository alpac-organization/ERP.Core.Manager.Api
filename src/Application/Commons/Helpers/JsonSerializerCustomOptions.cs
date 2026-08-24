using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Application.Commons.Helpers
{
    public static class JsonSerializerCustomOptions
    {
        public static readonly JsonSerializerOptions IgnoreCycles = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false
        };
    }
}