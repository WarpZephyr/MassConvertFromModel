using Assimp;

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
        /// Whether or not to do the check flip fix during FLVER0 face triangulation.
        /// </summary>
        public bool DoCheckFlip { get; set; } = false;

        /// <summary>
        /// Whether or not to automatically convert units depending on the export format.
        /// </summary>
        public bool ConvertUnitSystem { get; set; } = true;

        /// <summary>
        /// Whether or not to set unit system conversions into properties or metadata instead of manual conversions where possible.
        /// </summary>
        public bool PreferUnitSystemProperty { get; set; } = false;

        /// <summary>
        /// Fixes the root node of the scene before exporting.
        /// </summary>
        public bool FixRootNode { get; set; } = true;

        /// <summary>
        /// Whether or not to mirror the export across the X axis.
        /// </summary>
        public bool MirrorX { get; set; } = false;

        /// <summary>
        /// Whether or not to mirror the export across the Y axis.
        /// </summary>
        public bool MirrorY { get; set; } = false;

        /// <summary>
        /// Whether or not to mirror the export across the Z axis.
        /// </summary>
        public bool MirrorZ { get; set; } = true;

        /// <summary>
        /// The scale the export should be.
        /// </summary>
        public float Scale { get; set; } = 1.0f;

        /// <summary>
        /// The chosen export format.
        /// </summary>
        public string ExportFormat { get; set; } = "fbx";

        /// <summary>
        /// The export flags to use in assimp.
        /// </summary>
        public PostProcessSteps ExportFlags { get; set; } = PostProcessSteps.None;

        /// <summary>
        /// Parse a config from the given path.
        /// </summary>
        /// <param name="path">A file path to the config to parse.</param>
        public void Parse(string path)
        {
            var parser = new DictionaryConfigParser();
            parser.Parse(path);

            OutputToConsole = parser.SearchBoolProperty(OutputToConsole);
            OutputToLog = parser.SearchBoolProperty(OutputToLog);
            OutputTexturesFound = parser.SearchBoolProperty(OutputTexturesFound);
            ReplaceExistingFiles = parser.SearchBoolProperty(ReplaceExistingFiles);
            SearchForMDL4 = parser.SearchBoolProperty(SearchForMDL4);
            SearchForSMD4 = parser.SearchBoolProperty(SearchForSMD4);
            SearchForFlver0 = parser.SearchBoolProperty(SearchForFlver0);
            SearchForFlver2 = parser.SearchBoolProperty(SearchForFlver2);
            SearchForTextures = parser.SearchBoolProperty(SearchForTextures);
            SearchBND3 = parser.SearchBoolProperty(SearchBND3);
            SearchBND4 = parser.SearchBoolProperty(SearchBND4);
            SearchZero3 = parser.SearchBoolProperty(SearchZero3);
            BinderRecursiveSearch = parser.SearchBoolProperty(BinderRecursiveSearch);
            DoCheckFlip = parser.SearchBoolProperty(DoCheckFlip);
            ConvertUnitSystem = parser.SearchBoolProperty(ConvertUnitSystem);
            PreferUnitSystemProperty = parser.SearchBoolProperty(PreferUnitSystemProperty);
            FixRootNode = parser.SearchBoolProperty(FixRootNode);
            MirrorX = parser.SearchBoolProperty(MirrorX);
            MirrorY = parser.SearchBoolProperty(MirrorY);
            MirrorZ = parser.SearchBoolProperty(MirrorZ);
            Scale = parser.SearchFloatProperty(Scale);
            ExportFormat = parser.SearchStringProperty(ExportFormat);
        }
    }
}
