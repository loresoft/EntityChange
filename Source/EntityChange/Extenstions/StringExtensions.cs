using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EntityChange.Extenstions
{
    public static class StringExtensions
    {
        private static readonly Regex _splitNameRegex = new Regex(@"[\W_]+");
        private static readonly Regex _properWordRegex = new Regex(@"([A-Z][a-z]*)|([0-9]+)");
        private static readonly Regex _whitespace = new Regex(@"\s");
        private static readonly Regex _nameExpression = new Regex("([A-Z]+(?=$|[A-Z][a-z])|[A-Z]?[a-z]+)", RegexOptions.Compiled);

        /// <summary>
        /// Truncates the specified text.
        /// </summary>
        /// <param name="text">The text to truncate.</param>
        /// <param name="keep">The number of characters to keep.</param>
        /// <param name="ellipsis">The ellipsis string to use when truncating. (Default ...)</param>
        /// <returns>
        /// A truncate string.
        /// </returns>
        public static string Truncate(this string text, int keep, string ellipsis = "...")
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (string.IsNullOrEmpty(ellipsis))
                ellipsis = string.Empty;

            string buffer = NormalizeLineEndings(text);
            if (buffer.Length <= keep)
                return buffer;

            if (buffer.Length <= keep + ellipsis.Length || keep < ellipsis.Length)
                return buffer.Substring(0, keep);

            return string.Concat(buffer.Substring(0, keep - ellipsis.Length), ellipsis);
        }

        public static string NormalizeLineEndings(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text
                .Replace("\r\n", "\n")
                .Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// Indicates whether the specified String object is null or an empty string
        /// </summary>
        /// <param name="item">A String reference</param>
        /// <returns>
        ///     <c>true</c> if is null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty(this string item)
        {
            return string.IsNullOrEmpty(item);
        }

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters
        /// </summary>
        /// <param name="item">A String reference</param>
        /// <returns>
        ///      <c>true</c> if is null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrWhiteSpace(this string item)
        {
            if (item == null)
                return true;

            for (int i = 0; i < item.Length; i++)
                if (!char.IsWhiteSpace(item[i]))
                    return false;

            return true;
        }

        /// <summary>
        /// Determines whether the specified string is not <see cref="IsNullOrEmpty"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified <paramref name="value"/> is not <see cref="IsNullOrEmpty"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Uses the string as a format
        /// </summary>
        /// <param name="format">A String reference</param>
        /// <param name="args">Object parameters that should be formatted</param>
        /// <returns>Formatted string</returns>
        public static string FormatWith(this string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            return string.Format(format, args);
        }

        /// <summary>
        /// Applies a format to the item
        /// </summary>
        /// <param name="item">Item to format</param>
        /// <param name="format">Format string</param>
        /// <returns>Formatted string</returns>
        public static string FormatAs(this object item, string format)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            return string.Format(format, item);
        }

        /// <summary>
        /// Uses the string as a format.
        /// </summary>
        /// <param name="format">A String reference</param>
        /// <param name="source">Object that should be formatted</param>
        /// <returns>Formatted string</returns>
        public static string FormatName(this string format, object source)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            return NameFormatter.Format(format, source);
        }

        /// <summary>
        /// Applies a format to the item
        /// </summary>
        /// <param name="item">Item to format</param>
        /// <param name="format">Format string</param>
        /// <returns>Formatted string</returns>
        public static string FormatNameAs(this object item, string format)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            return NameFormatter.Format(format, item);
        }

        /// <summary>
        /// Converts a string to use camelCase.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The to camel case. </returns>
        public static string ToCamelCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            string output = ToPascalCase(value);
            if (output.Length > 2)
                return char.ToLower(output[0]) + output.Substring(1);

            return output.ToLower();
        }

        /// <summary>
        /// Converts a string to use PascalCase.
        /// </summary>
        /// <param name="value">Text to convert</param>
        /// <returns>The string</returns>
        public static string ToPascalCase(this string value)
        {
            return value.ToPascalCase(_splitNameRegex);
        }

        /// <summary>
        /// Converts a string to use PascalCase.
        /// </summary>
        /// <param name="value">Text to convert</param>
        /// <param name="splitRegex">Regular Expression to split words on.</param>
        /// <returns>The string</returns>
        public static string ToPascalCase(this string value, Regex splitRegex)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var mixedCase = value.IsMixedCase();
            var names = splitRegex.Split(value);
            var output = new StringBuilder();

            if (names.Length > 1)
            {
                foreach (string name in names)
                {
                    if (name.Length > 1)
                    {
                        output.Append(char.ToUpper(name[0]));
                        output.Append(mixedCase ? name.Substring(1) : name.Substring(1).ToLower());
                    }
                    else
                    {
                        output.Append(name);
                    }
                }
            }
            else if (value.Length > 1)
            {
                output.Append(char.ToUpper(value[0]));
                output.Append(mixedCase ? value.Substring(1) : value.Substring(1).ToLower());
            }
            else
            {
                output.Append(value.ToUpper());
            }

            return output.ToString();
        }

        /// <summary>
        /// Takes a NameIdentifier and spaces it out into words "Name Identifier".
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The string</returns>
        public static string ToSpacedWords(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            value = ToPascalCase(value);

            MatchCollection words = _properWordRegex.Matches(value);
            var spacedName = new StringBuilder();
            foreach (Match word in words)
            {
                spacedName.Append(word.Value);
                spacedName.Append(' ');
            }

            // remove last space
            spacedName.Length = spacedName.Length - 1;
            return spacedName.ToString();
        }

        /// <summary>
        /// Removes all whitespace from a string.
        /// </summary>
        /// <param name="s">Initial string.</param>
        /// <returns>String with no whitespace.</returns>
        public static string RemoveWhiteSpace(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return _whitespace.Replace(s, String.Empty);
        }

        /// <summary>
        /// Strips NewLines and Tabs
        /// </summary>
        /// <param name="s">The string to strip.</param>
        /// <returns>Stripped string.</returns>
        public static string RemoveInvisible(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s
                .Replace("\r\n", " ")
                .Replace('\n', ' ')
                .Replace('\t', ' ');
        }

        /// <summary>
        /// Appends a copy of the specified string followed by the default line terminator to the end of the StringBuilder object.
        /// </summary>
        /// <param name="sb">The StringBuilder instance to append to.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static StringBuilder AppendLine(this StringBuilder sb, string format, params object[] args)
        {
            sb.AppendFormat(format, args);
            sb.AppendLine();
            return sb;
        }

        /// <summary>
        /// Appends a copy of the specified string if <paramref name="condition"/> is met.
        /// </summary>
        /// <param name="sb">The StringBuilder instance to append to.</param>
        /// <param name="text">The string to append.</param>
        /// <param name="condition">The condition delegate to evaluate. If condition is null, String.IsNullOrWhiteSpace method will be used.</param>
        public static StringBuilder AppendIf(this StringBuilder sb, string text, Func<string, bool> condition = null)
        {
            var c = condition ?? (s => !string.IsNullOrWhiteSpace(s));

            if (c(text))
                sb.Append(text);

            return sb;
        }

        /// <summary>
        /// Appends a copy of the specified string followed by the default line terminator if <paramref name="condition"/> is met.
        /// </summary>
        /// <param name="sb">The StringBuilder instance to append to.</param>
        /// <param name="text">The string to append.</param>
        /// <param name="condition">The condition delegate to evaluate. If condition is null, String.IsNullOrWhiteSpace method will be used.</param>
        public static StringBuilder AppendLineIf(this StringBuilder sb, string text, Func<string, bool> condition = null)
        {
            var c = condition ?? (s => !string.IsNullOrWhiteSpace(s));

            if (c(text))
                sb.AppendLine(text);

            return sb;
        }

        /// <summary>
        /// Do any of the strings contain both uppercase and lowercase characters?
        /// </summary>
        /// <param name="values">String values.</param>
        /// <returns>True if any contain mixed cases.</returns>
        public static bool IsMixedCase(this IEnumerable<string> values)
        {
            return values.Any(value => value.IsMixedCase());
        }

        /// <summary>
        /// Does string contain both uppercase and lowercase characters?
        /// </summary>
        /// <param name="s">The value.</param>
        /// <returns>True if contain mixed case.</returns>
        public static bool IsMixedCase(this string s)
        {
            if (s.IsNullOrEmpty())
                return false;

            var containsUpper = s.Any(Char.IsUpper);
            var containsLower = s.Any(Char.IsLower);

            return containsLower && containsUpper;
        }
    }
}
