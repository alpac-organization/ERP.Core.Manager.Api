using NanoidDotNet;
using System.Text.RegularExpressions;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    public partial class CodeGenerator : ICodeGenerator
    {
        [GeneratedRegex(@"[^a-zA-Z]")]
        private static partial Regex GenerateModuleCode();

        public string GenerateModuleCode(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
                return $"GEN-{GetRandomSuffix()}";

            string cleanName = GenerateModuleCode().Replace(subject.Trim().ToUpper(), "");

            string prefix = cleanName.Length >= 3 
                ? cleanName[..3]
                : cleanName.PadRight(3, 'X');

            return $"{prefix}-{GetRandomSuffix()}";
        }

        public string GenerateUsername(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
                return "user.default";

            string cleanName = RemoveAccents(subject.ToLower().Trim());

            var parts = cleanName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1) return parts[0];

            string username = $"{parts[0]}.{parts[parts.Length - 1]}";

            return username;   
        }


        #region Metodos Privados
        private static string GetRandomSuffix()
        {
            const string alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
            return Nanoid.Generate(alphabet, size: 4);
        }

        private static string RemoveAccents(string text)
        {
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }
        #endregion Metodos Privado
    }
}