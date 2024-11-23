namespace MassConvertFromModel
{
    /// <summary>
    /// A parser for simple line reading while cleaning comments.
    /// </summary>
    public static class LineParser
    {
        /// <summary>
        /// Parse a single line in a config and clean it of unnecessary things.
        /// </summary>
        /// <param name="line">The line to parse.</param>
        /// <returns>The parsed line.</returns>
        public static string ParseLine(string line)
        {
            string parsedLine = line;
            int commentIndex = line.IndexOf("//");
            if (commentIndex == 0)
            {
                return string.Empty;
            }
            else if (commentIndex > 0)
            {
                parsedLine = parsedLine[..commentIndex];
            }

            parsedLine = parsedLine.Trim();
            return parsedLine;
        }
    }
}
