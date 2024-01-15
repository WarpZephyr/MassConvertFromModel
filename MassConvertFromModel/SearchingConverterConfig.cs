namespace MassConvertFromModel
{
    public class SearchingConverterConfig
    {
        public bool OutputToConsole { get; set; } = true;
        public bool OutputToLog { get; set; } = true;
        public bool OutputTexturesFound { get; set; } = true;
        public bool ReplaceExistingFiles { get; set; } = true;
        public bool SearchForMDL4 { get; set; } = true;
        public bool SearchForSMD4 { get; set; } = true;
        public bool SearchForFlver0 { get; set; } = true;
        public bool SearchForFlver2 { get; set; } = true;
        public bool SearchForTextures { get; set; } = true;
        public bool SearchBND3 { get; set; } = true;
        public bool SearchBND4 { get; set; } = true;
        public bool SearchZero3 { get; set; } = false;
        public bool BinderRecursiveSearch { get; set; } = true;
        public string ExportFormat { get; set; } = "fbx";

        public void Parse(string path)
        {
            var parser = new ConfigParser();
            parser.Parse(path);

            OutputToConsole = SearchBoolProperty(parser, nameof(OutputToConsole).ToLower(), OutputToConsole);
            OutputToLog = SearchBoolProperty(parser, nameof(OutputToLog).ToLower(), OutputToLog);
            OutputTexturesFound = SearchBoolProperty(parser, nameof(OutputTexturesFound).ToLower(), OutputTexturesFound);
            ReplaceExistingFiles = SearchBoolProperty(parser, nameof(ReplaceExistingFiles).ToLower(), ReplaceExistingFiles);
            SearchForMDL4 = SearchBoolProperty(parser, nameof(SearchForMDL4).ToLower(), SearchForMDL4);
            SearchForSMD4 = SearchBoolProperty(parser, nameof(SearchForSMD4).ToLower(), SearchForSMD4);
            SearchForFlver0 = SearchBoolProperty(parser, nameof(SearchForFlver0).ToLower(), SearchForFlver0);
            SearchForFlver2 = SearchBoolProperty(parser, nameof(SearchForFlver2).ToLower(), SearchForFlver2);
            SearchForTextures = SearchBoolProperty(parser, nameof(SearchForTextures).ToLower(), SearchForTextures);
            SearchBND3 = SearchBoolProperty(parser, nameof(SearchBND3).ToLower(), SearchBND3);
            SearchBND4 = SearchBoolProperty(parser, nameof(SearchBND4).ToLower(), SearchBND4);
            SearchZero3 = SearchBoolProperty(parser, nameof(SearchZero3).ToLower(), SearchZero3);
            BinderRecursiveSearch = SearchBoolProperty(parser, nameof(BinderRecursiveSearch).ToLower(), BinderRecursiveSearch);
            ExportFormat = SearchStringProperty(parser, nameof(ExportFormat).ToLower(), ExportFormat);
        }

        private bool SearchBoolProperty(ConfigParser parser, string propertyName, bool defaultResult)
        {
            if (parser.ValueDictionary.TryGetValue(propertyName, out string? str))
            {
                if (bool.TryParse(str, out bool result))
                {
                    return result;
                }
            }

            return defaultResult;
        }

        private string SearchStringProperty(ConfigParser parser, string propertyName, string defaultResult)
        {
            if (parser.ValueDictionary.TryGetValue(propertyName, out string? str))
            {
                if (str != null)
                {
                    return str;
                }
            }

            return defaultResult;
        }
    }
}
