using System;
using System.Globalization;

namespace Dashboard.Utils
{
    public class DateTimeUtil
    {
        private const string DbDateTimeFormat = "yyyyMMddHHmmss";
        private const string DisplayDateTimeFormat = "dd-MMM-yyyy h:mm:ss tt";
        private const string DisplayDateFormat = "dd-MMM-yyyy";

        public static DateTime GetDatetimeFromDbDateTime(string date)
        {
            return DateTime.ParseExact(date, DbDateTimeFormat, CultureInfo.InvariantCulture);
        }

        public static string GetDbDateTimeFromDateTime(DateTime dateTime)
        {
            return dateTime.ToString(DbDateTimeFormat);
        }

        public static string GetDisplayDateTimeFromDateTime(DateTime dateTime)
        {
            return dateTime.ToString(DisplayDateTimeFormat);
        }
        public static string GetDisplayDateFromDateTime(DateTime dateTime)
        {
            return dateTime.ToString(DisplayDateFormat);
        }
    }
}