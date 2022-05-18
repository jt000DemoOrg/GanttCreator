using System;

namespace Mechavian.WpfHelpers
{
    public static class DateTimeOffsetExtensions
    {
        public static bool Between(this DateTimeOffset dateTime, DateTimeOffset start, DateTimeOffset end)
        {
            if (start > end)
            {
                (start, end) = (end, start);
            }

            return dateTime >= start && dateTime <= end;
        }
    }
}