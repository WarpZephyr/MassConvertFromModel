namespace MassConvertFromModel
{
    public class SearchingConverterConfig
    {
        public bool OutputToConsole { get; set; } = true;
        public bool OutputToLog { get; set; } = true;
        public bool ReplaceExistingFiles { get; set; } = true;

        public void Parse(string path)
        {
            var parser = new ConfigParser();
            parser.Parse(path);

            if (parser.ValueDictionary.TryGetValue(nameof(OutputToConsole).ToLower(), out string? outputToConsoleStr))
            {
                if (bool.TryParse(outputToConsoleStr, out bool resultBool))
                {
                    OutputToConsole = resultBool;
                }
            }

            if (parser.ValueDictionary.TryGetValue(nameof(OutputToLog).ToLower(), out string? outputToLogStr))
            {
                if (bool.TryParse(outputToLogStr, out bool resultBool))
                {
                    OutputToLog = resultBool;
                }
            }

            if (parser.ValueDictionary.TryGetValue(nameof(ReplaceExistingFiles).ToLower(), out string? replaceExistingFiles))
            {
                if (bool.TryParse(replaceExistingFiles, out bool resultBool))
                {
                    ReplaceExistingFiles = resultBool;
                }
            }
        }
    }
}
