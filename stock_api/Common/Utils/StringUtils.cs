using System.Globalization;

namespace stock_api.Common.Utils
{
    public class StringUtils
    {
        public static string CapitalizeFirstLetter(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            // Use the current culture to handle casing rules
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;

            // Capitalize the first letter
            return textInfo.ToTitleCase(str[0].ToString()) + str.Substring(1);
        }
    }
}
