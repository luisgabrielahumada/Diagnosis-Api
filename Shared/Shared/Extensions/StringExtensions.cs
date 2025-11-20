using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared.Extensions
{
    public static class StringExtensions
    {
        private static readonly Dictionary<string, int> _counterMap = new();
        public static bool ToBool(this string value)
        {
            bool ret;
            bool.TryParse(value, out ret);
            return ret;
        }

        public static int ToInt(this string value)
        {
            int ret;
            int.TryParse(value, out ret);
            return ret;
        }
        public static int ToInt(this int? value)
        {
            return value.IsNull(0).Value;
        }

        public static bool IsNullOrEmpty(this string item)
        {
            return string.IsNullOrEmpty(item);
        }
        public static string IsNullOrEmpty(this string item, string result)
        {
            return string.IsNullOrEmpty(item) ? result : item;
        }
        public static bool IsNullOrWhiteSpace(this string item)
        {
            return string.IsNullOrWhiteSpace(item);
        }
        public static string IsNullOrWhiteSpace(this string item, string result)
        {
            return string.IsNullOrWhiteSpace(item) ? result : item;
        }

        public static bool IsNotNullOrEmpty(this string item)
        {
            return !string.IsNullOrEmpty(item);
        }
        public static string IsNotNullOrEmpty(this string item, string result)
        {
            return !string.IsNullOrEmpty(item) ? result : item;
        }
        public static bool IsNotNullOrWhiteSpace(this string item)
        {
            return !string.IsNullOrWhiteSpace(item);
        }
        public static string IsNotNullOrWhiteSpace(this string item, string result)
        {
            return !string.IsNullOrWhiteSpace(item) ? result : item;
        }

        public static string EncodeBase64(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string DecodeBase64(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string RemoveDiacritics(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            var sb = new StringBuilder();
            foreach (var c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string ReplaceFields(this string value, KeyValuePair<string, string>[] fields)
        {
            foreach (var field in fields)
                value = value.Replace("[" + field.Key + "]", field.Value);

            return value;
        }

        public static bool IsOnlyNumbers(this string str)
        {
            var valid = true;
            foreach (var c in str)
            {
                if (!(c >= '0' && c <= '9'))
                    valid = false;
            }
            return valid;
        }

        public static bool IsOnlyLetters(this string str)
        {
            var regex = new Regex(@"^[\p{L} ]+$");
            return regex.IsMatch(str);
        }

        public static bool IsOnlyLettersAndNumbers(this string str)
        {
            var valid = true;
            foreach (var c in str)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                    valid = false;
            }
            return valid;
        }

        public static bool IsEmail(this string str)
        {
            var regex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            return regex.IsMatch(str);
        }

        public static DateTime DecryptDateTimeExact(this string value, string format)
        {
            return DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
        }
        public static string PeriodYear(this string name, DateTime startDate, DateTime EndDate)
        {
            var rangeYear = startDate.Year.ToString();
            if (EndDate.Year > startDate.Year)
                rangeYear = $"{startDate.Year}-{EndDate.Year}";

            return $"{name}-{rangeYear}";
        }

        public static int GetConsecutive(this DateTime date, Guid planningId)
        {
            string key = $"{planningId}_{date:yyyyMMdd}";

            if (!_counterMap.ContainsKey(key))
                _counterMap[key] = 1;
            else
                _counterMap[key]++;

            return _counterMap[key];
        }

        public static string GenerateSecurePassword(int length = 12)
        {
            if (length < 9)
                throw new ArgumentException("La longitud mínima debe ser de 9 caracteres.");

            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string symbols = "!@#$%^&*()-_=+[]{}|;:,.<>?";

            var allChars = upper + lower + digits + symbols;
            var rand = new Random();

            // Aseguramos al menos uno de cada tipo
            var password = new List<char>
            {
                upper[rand.Next(upper.Length)],
                lower[rand.Next(lower.Length)],
                digits[rand.Next(digits.Length)],
                symbols[rand.Next(symbols.Length)]
            };

            // Completar el resto de forma aleatoria
            for (int i = password.Count; i < length; i++)
            {
                password.Add(allChars[rand.Next(allChars.Length)]);
            }

            // Mezclar la contraseña para evitar patrón predecible
            return new string(password.OrderBy(x => rand.Next()).ToArray());
        }

        public static T ToEnum<T>(this string value, bool ignoreCase = true) where T : struct
        {
            if (Enum.TryParse<T>(value, ignoreCase, out var result))
                return result;

            throw new ArgumentException($"'{value}' no es un valor válido para {typeof(T).Name}");
        }

        public static string ReplacePlaceholders(this string? content, Dictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(content) || values == null)
                return content;

            foreach (var pair in values)
            {
                string placeholder = $"{{{{{pair.Key}}}}}"; // genera {{nombre}}
                content = content.Replace(placeholder, pair.Value ?? string.Empty);
            }
            return content;
        }
        public static string SanitizeFileName(this string name)
           => string.Concat(name.Split(Path.GetInvalidFileNameChars()));

        private static string ExpandPathTokens(string path)
        {
            var expanded = Environment.ExpandEnvironmentVariables(path ?? string.Empty);
            if (expanded.StartsWith("~"))
            {
                var baseDir = AppContext.BaseDirectory;
                expanded = Path.Combine(baseDir, expanded.TrimStart('~', '/', '\\'));
            }
            return expanded;
        }
 

        public static string NormalizeZipPath(this string path) => (path ?? string.Empty).Replace('\\', '/');

        public static bool IsDirectoryEntry(this string normalized) =>
            normalized.EndsWith("/", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(Path.GetFileName(normalized));

        public static string StripSingleTopFolder(this string normalized)
        {
            if (string.IsNullOrWhiteSpace(normalized)) return normalized;
            var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 1) return normalized;

            return string.Join('/', parts.Skip(1));
        }
    }
}