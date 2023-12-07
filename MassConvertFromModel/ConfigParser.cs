using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassConvertFromModel
{
    public class ConfigParser
    {
        public Dictionary<string, string> ValueDictionary = new Dictionary<string, string>();

        public void Parse(string path)
        {
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string parsedLine = ParseLine(line);
                if (!string.IsNullOrEmpty(parsedLine))
                {
                    ParseValue(parsedLine);
                }
            }
        }

        private string ParseLine(string line)
        {
            string parsedLine = line;
            int commentIndex = line.IndexOf("//");
            if (commentIndex == 0)
            {
                return string.Empty;
            }
            else if (commentIndex > 0)
            {
                parsedLine = parsedLine.Substring(0, commentIndex);
            }

            parsedLine = parsedLine.Trim();
            return parsedLine;
        }

        private void ParseValue(string parsedLine)
        {
            string[] strs = parsedLine.Split('=', StringSplitOptions.TrimEntries);

            if (strs.Length > 1)
            {
                ValueDictionary.Add(strs[0].ToLower(), strs[1]);
            }
        }
    }
}
