using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeDetector.Extenstions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Converts an IEnumerable of values to a delimited string.
        /// </summary>
        /// <typeparam name="T">The type of objects to delimit.</typeparam>
        /// <param name="values">The IEnumerable of values to convert.</param>
        /// <param name="delimiter">The string delimiter.</param>
        /// <param name="escapeDelimiter">A delegate used to escape the delimiter contained in the value.</param>
        /// <returns>
        /// A delimited string of the values.
        /// </returns>
        public static string ToDelimitedString<T>(this IEnumerable<T> values, string delimiter = ",", Func<string, string> escapeDelimiter = null)
        {
            var sb = new StringBuilder();
            foreach (var value in values)
            {
                if (sb.Length > 0)
                    sb.Append(delimiter);

                var v = value?.ToString() ?? string.Empty;
                if (escapeDelimiter != null)
                    v = escapeDelimiter(v);

                sb.Append(v);
            }

            return sb.ToString();
        }

    }

}
