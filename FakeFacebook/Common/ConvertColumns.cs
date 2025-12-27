using System.Text.RegularExpressions;

namespace FakeFacebook.Common
{
    public static class ConvertColumns
    {
        public static string ToSnakeCase(this string input, bool isUpper = false)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            var startUnderscores = Regex.Match(input, @"^_+");

            return isUpper ? startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToUpper()
                           : startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }
}
