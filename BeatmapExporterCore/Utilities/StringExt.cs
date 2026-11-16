namespace BeatmapExporterCore.Utilities
{
    public static class StringExt
    {
        /// <summary>
        /// Truncates a string to the specified length of characters
        /// </summary>
        public static string Trunc(this string str, int len)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= len) return str;
            // Do not split a UTF-16 surrogate pair
            if (len > 0 && char.IsHighSurrogate(str[len - 1])) len--;
            return str[..len];
        }

        private static readonly HashSet<string> reservedNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
        };

        /// <summary>
        /// Removes characters that should not be in a (Windows) file name.
        /// The result is a single path segment: it never contains a directory separator,
        /// is never "." or "..", and does not end with a dot or space.
        /// </summary>
        public static string RemoveFilenameCharacters(this string str)
        {
            var sb = new System.Text.StringBuilder(str.Length);
            foreach (char c in str)
            {
                switch (c)
                {
                    case '"' or '*' or '<' or '>' or ':' or '/' or '\\' or '|' or '?':
                        break;
                    default:
                        if (!char.IsControl(c)) sb.Append(c);
                        break;
                }
            }

            // Windows does not allow trailing dots or spaces, this also removes "." and ".."
            var name = sb.ToString().TrimEnd('.', ' ').TrimStart(' ');
            if (name.Length == 0) return "_";

            // Windows reserved device names, also when followed by an extension ("NUL.mp3")
            var stem = name.Split('.')[0].TrimEnd(' ');
            if (reservedNames.Contains(stem)) name = "_" + name;

            return name;
        }

        /// <summary>
        /// Returns an array of strings which split the original string by comma (,)
        /// </summary>
        public static string[] CommaSeparatedArg(this string str) => str.Split(",").Select(s => s.Trim()).ToArray();
    }
}
