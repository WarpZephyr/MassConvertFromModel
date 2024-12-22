using System.Runtime.CompilerServices;

namespace MassConvertFromModel.Configs
{
    /// <summary>
    /// A parser for simple configuration files.
    /// </summary>
    internal class DictionaryConfigParser
    {
        /// <summary>
        /// A dictionary containing read property to value pairs.
        /// </summary>
        public Dictionary<string, string> ValueDictionary = [];

        /// <summary>
        /// Parse a config from the given path.
        /// </summary>
        /// <param name="path">A file path to the config to parse.</param>
        public void Parse(string path)
        {
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string parsedLine = LineParser.ParseLine(line);
                if (!string.IsNullOrEmpty(parsedLine))
                {
                    ParseValue(parsedLine);
                }
            }
        }

        /// <summary>
        /// Parse a property and value pair from a single line for a config.
        /// </summary>
        /// <param name="parsedLine">The line to parse the property and value of.</param>
        private void ParseValue(string parsedLine)
        {
            string[] strs = parsedLine.Split('=', StringSplitOptions.TrimEntries);

            if (strs.Length > 1)
            {
                ValueDictionary.Add(strs[0].ToLowerInvariant(), strs[1]);
            }
        }

        /// <summary>
        /// Search for a <see cref="bool"/> value.
        /// </summary>
        /// <param name="value">The value and the default for it should it not be found.</param>
        /// <param name="name">The name of the value to search for.</param>
        /// <returns>The found value or the default.</returns>
        public float SearchFloatProperty(float value, [CallerArgumentExpression(nameof(value))] string name = "")
        {
            if (ValueDictionary.TryGetValue(name.ToLowerInvariant(), out string? str))
            {
                if (float.TryParse(str, out float result))
                {
                    return result;
                }
            }

            return value;
        }

        /// <summary>
        /// Search for a <see cref="bool"/> value.
        /// </summary>
        /// <param name="value">The value and the default for it should it not be found.</param>
        /// <param name="name">The name of the value to search for.</param>
        /// <returns>The found value or the default.</returns>
        public bool SearchBoolProperty(bool value, [CallerArgumentExpression(nameof(value))] string name = "")
        {
            if (ValueDictionary.TryGetValue(name.ToLowerInvariant(), out string? str))
            {
                if (bool.TryParse(str, out bool result))
                {
                    return result;
                }
            }

            return value;
        }

        /// <summary>
        /// Search for a <see cref="string"/> value.
        /// </summary>
        /// <param name="value">The value and the default for it should it not be found.</param>
        /// <param name="name">The name of the value to search for.</param>
        /// <returns>The found value or the default.</returns>
        public string SearchStringProperty(string value, [CallerArgumentExpression(nameof(value))] string name = "")
        {
            if (ValueDictionary.TryGetValue(name.ToLowerInvariant(), out string? str))
            {
                if (str != null)
                {
                    return str;
                }
            }

            return value;
        }
    }
}
