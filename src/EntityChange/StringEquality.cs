using System;

namespace EntityChange
{
    /// <summary>
    /// A <see langword="static"/> class for string equality comparison
    /// </summary>
    public static class StringEquality
    {
        /// <summary>
        /// Determines whether the specified <paramref name="original"/> and <paramref name="current"/> 
        /// strings have the same value using <see cref="StringComparison.CurrentCulture"/> option.
        /// </summary>
        /// <param name="original">The original string to compare.</param>
        /// <param name="current">The current string to compare.</param>
        /// <returns><c>true</c> if <paramref name="original"/> is equal <paramref name="current"/>; otherwise, <c>false</c>.</returns>
        public static bool CurrentCulture(object original, object current)
        {
            string o = original as string;
            string c = current as string;

            return string.Equals(o, c, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="original"/> and <paramref name="current"/> 
        /// strings have the same value using <see cref="StringComparison.CurrentCultureIgnoreCase"/> option.
        /// </summary>
        /// <param name="original">The original string to compare.</param>
        /// <param name="current">The current string to compare.</param>
        /// <returns><c>true</c> if <paramref name="original"/> is equal <paramref name="current"/>; otherwise, <c>false</c>.</returns>
        public static bool CurrentCultureIgnoreCase(object original, object current)
        {
            string o = original as string;
            string c = current as string;

            return string.Equals(o, c, StringComparison.CurrentCultureIgnoreCase);
        }

#if !NETSTANDARD1_0 && !NETSTANDARD1_5
        /// <summary>
        /// Determines whether the specified <paramref name="original"/> and <paramref name="current"/> 
        /// strings have the same value using <see cref="StringComparison.InvariantCulture"/> option.
        /// </summary>
        /// <param name="original">The original string to compare.</param>
        /// <param name="current">The current string to compare.</param>
        /// <returns><c>true</c> if <paramref name="original"/> is equal <paramref name="current"/>; otherwise, <c>false</c>.</returns>
        public static bool InvariantCulture(object original, object current)
        {
            string o = original as string;
            string c = current as string;

            return string.Equals(o, c, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="original"/> and <paramref name="current"/> 
        /// strings have the same value using <see cref="StringComparison.InvariantCultureIgnoreCase"/> option.
        /// </summary>
        /// <param name="original">The original string to compare.</param>
        /// <param name="current">The current string to compare.</param>
        /// <returns><c>true</c> if <paramref name="original"/> is equal <paramref name="current"/>; otherwise, <c>false</c>.</returns>
        public static bool InvariantCultureIgnoreCase(object original, object current)
        {
            string o = original as string;
            string c = current as string;

            return string.Equals(o, c, StringComparison.InvariantCultureIgnoreCase);
        }
#endif

        /// <summary>
        /// Determines whether the specified <paramref name="original"/> and <paramref name="current"/> 
        /// strings have the same value using <see cref="StringComparison.Ordinal"/> option.
        /// </summary>
        /// <param name="original">The original string to compare.</param>
        /// <param name="current">The current string to compare.</param>
        /// <returns><c>true</c> if <paramref name="original"/> is equal <paramref name="current"/>; otherwise, <c>false</c>.</returns>
        public static bool Ordinal(object original, object current)
        {
            string o = original as string;
            string c = current as string;

            return string.Equals(o, c, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="original"/> and <paramref name="current"/> 
        /// strings have the same value using <see cref="StringComparison.OrdinalIgnoreCase"/> option.
        /// </summary>
        /// <param name="original">The original string to compare.</param>
        /// <param name="current">The current string to compare.</param>
        /// <returns><c>true</c> if <paramref name="original"/> is equal <paramref name="current"/>; otherwise, <c>false</c>.</returns>
        public static bool OrdinalIgnoreCase(object original, object current)
        {
            string o = original as string;
            string c = current as string;

            return string.Equals(o, c, StringComparison.OrdinalIgnoreCase);
        }
    }
}