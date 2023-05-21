using DNTPersianUtils.Core;

namespace Project.Application.Extensions
{
    public static class StringExtensions
    {
        public static string NormalizeText(this string baseValue)
        {
            if (!string.IsNullOrWhiteSpace(baseValue))
                return baseValue
                    .ToLower()
                    .Trim()
                    .ToEnglishNumbers();
            return "";
        }

        public static string ThousandSeperator(this int baseValue)
        {
            return baseValue.ToString("#,##0");
        }
    }
}
