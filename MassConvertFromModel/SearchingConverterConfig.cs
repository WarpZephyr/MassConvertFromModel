namespace MassConvertFromModel
{
    /// <summary>
    /// A configuration for a <see cref="SearchingConverter"/>.
    /// </summary>
    public class SearchingConverterConfig
    {
        /// <summary>
        /// Whether or not logged events are outputted to the console.
        /// </summary>
        public bool OutputToConsole { get; set; } = true;

        /// <summary>
        /// Whether or not logged events are outputted to a log file.
        /// </summary>
        public bool OutputToLog { get; set; } = true;

        /// <summary>
        /// Whether or not outputs include textures that were found.
        /// </summary>
        public bool OutputTexturesFound { get; set; } = true;

        /// <summary>
        /// Whether or not to replace already existing files when converting.
        /// </summary>
        public bool ReplaceExistingFiles { get; set; } = true;

        /// <summary>
        /// Whether or not to search for <see cref="SoulsFormats.MDL4"/> models.
        /// </summary>
        public bool SearchForMDL4 { get; set; } = true;

        /// <summary>
        /// Whether or not to search for <see cref="SoulsFormats.SMD4"/> models.
        /// </summary>
        public bool SearchForSMD4 { get; set; } = true;

        /// <summary>
        /// Whether or not to search for <see cref="SoulsFormats.FLVER0"/> models.
        /// </summary>
        public bool SearchForFlver0 { get; set; } = true;

        /// <summary>
        /// Whether or not to search for <see cref="SoulsFormats.FLVER2"/> models.
        /// </summary>
        public bool SearchForFlver2 { get; set; } = true;

        /// <summary>
        /// Whether or not to search for textures.
        /// </summary>
        public bool SearchForTextures { get; set; } = true;

        /// <summary>
        /// Whether or not to search through <see cref="SoulsFormats.BND3"/> archives.
        /// </summary>
        public bool SearchBND3 { get; set; } = true;

        /// <summary>
        /// Whether or not to search through <see cref="SoulsFormats.BND4"/> archives.
        /// </summary>
        public bool SearchBND4 { get; set; } = true;

        /// <summary>
        /// Whether or not to search through <see cref="SoulsFormats.AC4.Zero3"/> archives.
        /// </summary>
        public bool SearchZero3 { get; set; } = false;

        /// <summary>
        /// Whether or not to recursively search binder archives.
        /// </summary>
        public bool BinderRecursiveSearch { get; set; } = true;

        /// <summary>
        /// The chosen export format.
        /// </summary>
        public string ExportFormat { get; set; } = "fbx";

        /// <summary>
        /// Parse a config from the given path.
        /// </summary>
        /// <param name="path">A file path to the config to parse.</param>
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

        /// <summary>
        /// Search for a <see cref="bool"/> property.
        /// </summary>
        /// <param name="parser">The config parser.</param>
        /// <param name="propertyName">The name of the property to search for.</param>
        /// <param name="defaultResult">The default should the property not be found.</param>
        /// <returns>The found property or the default.</returns>
        private static bool SearchBoolProperty(ConfigParser parser, string propertyName, bool defaultResult)
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

        /// <summary>
        /// Search for a <see cref="string"/> property.
        /// </summary>
        /// <param name="parser">The config parser.</param>
        /// <param name="propertyName">The name of the property to search for.</param>
        /// <param name="defaultResult">The default should the property not be found.</param>
        /// <returns>The found property or the default.</returns>
        private static string SearchStringProperty(ConfigParser parser, string propertyName, string defaultResult)
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
