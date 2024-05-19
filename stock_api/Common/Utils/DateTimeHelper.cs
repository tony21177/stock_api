using System.Globalization;

namespace stock_api.Common.Utils
{
    public class DateTimeHelper
    {
        public static DateTime? ParseTaiwanDateString(string? dateString)
        {
            CultureInfo culture = new("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            if (DateTime.TryParseExact(dateString, "yyy/M/dd", culture, DateTimeStyles.None, out DateTime result)
        || DateTime.TryParseExact(dateString, "yyy/MM/dd", culture, DateTimeStyles.None, out result))
            {
                return result;
            }
            return null;
        }

        public static DateTime? ParseDateString(string? dateString)
        {

            if (DateTime.TryParseExact(dateString, "yyyy/M/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)
        || DateTime.TryParseExact(dateString, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }
            return null;
        }

        public static DateTime? ParseDateStringForDash(string? dateString)
        {

            if (DateTime.TryParseExact(dateString, "yyyy-M-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)
        || DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }
            return null;
        }


        public static DateTime? ParseDateTimeFromUnixTime(long? dateTime)
        {
            if (dateTime.HasValue)
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(dateTime.Value).UtcDateTime;
            }

            return null;
        }

        public static long ConvertToUnixTimestamp(DateTime? dateTime)
        {
            if (dateTime == null) return 0;
            DateTimeOffset dateTimeOffset = new DateTimeOffset((DateTime)dateTime);
            return dateTimeOffset.ToUnixTimeMilliseconds();
        }

        public static string? FormatDateString(DateTime? dateTime)
        {
            CultureInfo culture = new("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            if (dateTime.HasValue)
            {
                return dateTime.Value.ToString("yyy/MM/dd", culture);
            }
            return null;
        }

        public static string? FormatDateString(DateTime? dateTime,string dateTimeFormatString)
        {
            CultureInfo culture = new("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            if (dateTime.HasValue)
            {
                return dateTime.Value.ToString(dateTimeFormatString, culture);
            }
            return null;
        }
    }
}
