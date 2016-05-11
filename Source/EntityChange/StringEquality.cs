using System;

namespace EntityChange
{
    public static class StringEquality
    {
        public static bool CurrentCulture(object original, object current)
        {
            string o = original as string;
            string c = current as string;

            return string.Equals(o, c, StringComparison.CurrentCulture);
        }

        public static bool CurrentCultureIgnoreCase(object original, object current)
        {
            string o = original as string;
            string c = current as string;

            return string.Equals(o, c, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool InvariantCulture(object original, object current)
        {
            string o = original as string;
            string c = current as string;

            return string.Equals(o, c, StringComparison.InvariantCulture);
        }

        public static bool InvariantCultureIgnoreCase(object original, object current)
        {
            string o = original as string;
            string c = current as string;

            return string.Equals(o, c, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool Ordinal(object original, object current)
        {
            string o = original as string;
            string c = current as string;

            return string.Equals(o, c, StringComparison.Ordinal);
        }

        public static bool OrdinalIgnoreCase(object original, object current)
        {
            string o = original as string;
            string c = current as string;

            return string.Equals(o, c, StringComparison.OrdinalIgnoreCase);
        }
    }
}