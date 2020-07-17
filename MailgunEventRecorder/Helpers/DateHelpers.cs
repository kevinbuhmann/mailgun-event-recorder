namespace MailgunEventRecorder.Helpers
{
    using System;

    public static class DateHelpers
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

        public static DateTime MinDate(DateTime date1, DateTime date2)
        {
            return date1 < date2 ? date1 : date2;
        }

        public static DateTime MaxDate(DateTime date1, DateTime date2)
        {
            return date1 > date2 ? date1 : date2;
        }

        public static double ToUnixTimeStamp(DateTime utcDate)
        {
            return utcDate.Subtract(UnixEpoch).TotalSeconds;
        }
    }
}
