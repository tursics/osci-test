using System;
using System.Globalization;
using System.Text;

namespace Osci.MessageParts
{
    /// <summary>
    /// Zusammenfassung für ISO8601DateTimeFormat.
    /// </summary>
    public class Iso8601DateTimeFormat
    {
        public string Format(DateTime date)
        {
            StringBuilder sb = Format(date, new StringBuilder());
            return sb.ToString();
        }
        private static StringBuilder Format(DateTime date, StringBuilder sbuf)
        {
            DateTime utcdt = date.ToUniversalTime();
            sbuf.Append(date.ToString(CultureInfo.InvariantCulture.DateTimeFormat.SortableDateTimePattern));
            TimeSpan ts = date - utcdt;
            sbuf.Append(ts.Hours < 0 ? "-" : "+");
            sbuf.Append(Math.Abs(ts.Hours) > 9 ? "" : "0");
            sbuf.Append(ts.Hours + ":");
            sbuf.Append(Math.Abs(ts.Minutes) > 9 ? "" : "0");
            sbuf.Append(ts.Minutes);
            return sbuf;
        }
        public DateTime Parse(string s)
        {
            return DateTime.Parse(s);
        }
        public DateTimeFormatInfo MakeFormat()
        {
            return new DateTimeFormatInfo();
        }
    }
}
