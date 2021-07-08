using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthwindDataLayer.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime Convert(this DateTimeOffset d)
        {
            return d.DateTime;
        }

        public static DateTime? Convert(this DateTimeOffset? d)
        {
            if (!d.HasValue) return null;
            return d.Value.DateTime;
        }

        public static DateTimeOffset Convert(this DateTime d)
        {
            return new DateTimeOffset(d);
        }

        public static DateTimeOffset? Convert(this DateTime? d)
        {
            if (d == null) return null;
            return new DateTimeOffset(d.Value);
        }

    }
}
