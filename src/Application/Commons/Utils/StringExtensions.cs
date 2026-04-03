using System.Globalization;

namespace ERP.Core.Manager.Api.Application.Commons.Utils
{
    public static class StringExtensions
    {
        public static string ToCapitalize(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            return value.Trim().ToLower() switch
            {
                "" => string.Empty,
                string s => char.ToUpper(s[0]) + s[1..]
            };
        }

        public static string Capitalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        }
    }
}
