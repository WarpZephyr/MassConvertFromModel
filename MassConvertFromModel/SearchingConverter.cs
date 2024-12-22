using Assimp;
using FromAssimp;
using MassConvertFromModel.Handlers;
using MassConvertFromModel.Loggers;
using SoulsFormats;
using System.Diagnostics.CodeAnalysis;

namespace MassConvertFromModel
{
    /// <summary>
    /// Searches through files and archives recursively to convert found models and textures.
    /// </summary>
    public class SearchingConverter
    {
        /// <summary>
        /// The format cache for supported assimp formats.
        /// </summary>
        private ContextFormatCache FormatCache { get; set; }

        /// <summary>
        /// The assimp context converter.
        /// </summary>
        public FromAssimpContext Context { get; set; }

        /// <summary>
        /// The logger.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// The export flags for the assimp context converter.
        /// </summary>
        public PostProcessSteps ExportFlags { get; set; }

        /// <summary>
        /// The export format.
        /// </summary>
        public string ExportFormat { get; set; }

        /// <summary>
        /// The root folder to be testing in.
        /// </summary>
        public string RootFolder { get; set; }

        /// <summary>
        /// The folder to replace the root folder with for pathing output.
        /// </summary>
        public string RootFolderOverride { get; set; }

        /// <summary>
        /// Whether or not to search for <see cref="SMD4"/> models.
        /// </summary>
        public bool SearchForSmd4 { get; set; }

        /// <summary>
        /// Whether or not to search for <see cref="MDL4"/> models.
        /// </summary>
        public bool SearchForMdl4 { get; set; }

        /// <summary>
        /// Whether or not to search for <see cref="FLVER0"/> models.
        /// </summary>
        public bool SearchForFlver0 { get; set; }

        /// <summary>
        /// Whether or not to search for <see cref="FLVER2"/> models.
        /// </summary>
        public bool SearchForFlver2 { get; set; }

        /// <summary>
        /// Whether or not to search for <see cref="TPF"/> textures.
        /// </summary>
        public bool SearchForTpf { get; set; }

        /// <summary>
        /// Whether or not to search <see cref="BND3"/> archives.
        /// </summary>
        public bool SearchForBND3 { get; set; }

        /// <summary>
        /// Whether or not to search <see cref="BND4"/> archives.
        /// </summary>
        public bool SearchForBND4 { get; set; }

        /// <summary>
        /// Whether or not to search <see cref="BXF3"/> archives.
        /// </summary>
        public bool SearchForBXF3 { get; set; }

        /// <summary>
        /// Whether or not to search <see cref="BXF4"/> archives.
        /// </summary>
        public bool SearchForBXF4 { get; set; }

        /// <summary>
        /// Whether or not to search <see cref="Zero3"/> archives.
        /// </summary>
        public bool SearchForZero3 { get; set; }

        /// <summary>
        /// Whether or not to recursively search archives.
        /// </summary>
        public bool RecursiveSearch { get; set; }

        /// <summary>
        /// Whether or not to replace existing files.
        /// </summary>
        public bool ReplaceExistingFiles { get; set; }

        /// <summary>
        /// Fixes the root node of the scene before exporting if necessary.
        /// </summary>
        public bool FixRootNode { get; set; }

        /// <summary>
        /// Whether or not to use the root folder properties.
        /// </summary>
        public bool UseRootFolder { get; set; }

        /// <summary>
        /// Whether or not to copy the file being imported to the export location.
        /// </summary>
        public bool CopyImport { get; set; }

        /// <summary>
        /// Whether or not to log when models are exported.
        /// </summary>
        public bool LogModelsExported { get; set; }

        /// <summary>
        /// Whether or not to log when textures are exported.
        /// </summary>
        public bool LogTexturesExported { get; set; }

        /// <summary>
        /// Whether or not to log when models are copied.
        /// </summary>
        public bool LogModelsCopied { get; set; }

        /// <summary>
        /// Whether or not to log when textures are copied.
        /// </summary>
        public bool LogTexturesCopied { get; set; }

        /// <summary>
        /// Whether or not to log warnings.
        /// </summary>
        public bool LogWarnings { get; set; }

        /// <summary>
        /// Whether or not to log errors.
        /// </summary>
        public bool LogErrors { get; set; }

        /// <summary>
        /// Create a new <see cref="SearchingConverter"/>.
        /// </summary>
        /// <param name="context">The assimp context converter.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="enableLogging">Whether or not to log things.</param>
        public SearchingConverter(FromAssimpContext context, ILogger logger, bool enableLogging = true)
        {
            FormatCache = new ContextFormatCache(context.Context);
            Context = context;
            Logger = logger;
            ExportFlags = PostProcessSteps.None;
            ExportFormat = "fbx";
            RootFolder = string.Empty;
            RootFolderOverride = string.Empty;
            SearchForSmd4 = true;
            SearchForMdl4 = true;
            SearchForFlver0 = true;
            SearchForFlver2 = true;
            SearchForTpf = true;
            SearchForBND3 = true;
            SearchForBND4 = true;
            SearchForBXF3 = true;
            SearchForBXF4 = true;
            SearchForZero3 = true;
            RecursiveSearch = true;
            ReplaceExistingFiles = true;
            UseRootFolder = false;
            CopyImport = false;
            LogModelsExported = enableLogging;
            LogTexturesExported = enableLogging;
            LogModelsCopied = enableLogging;
            LogTexturesCopied = enableLogging;
            LogWarnings = enableLogging;
            LogErrors = enableLogging;
        }

        #region Search

        /// <summary>
        /// Search a folder.
        /// </summary>
        /// <param name="folder">The folder to search.</param>
        public void SearchFolder(string folder)
        {
            string exportFormat = GetExportFormat();
            string outExtension = FromAssimpContext.GetFormatExtension(exportFormat);
            var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string fileFolder = PathHandler.GetDirectoryName(file);
                string outFolder = GetOutputFolder(fileFolder);
                string outName = Path.GetFileName(file);
                SearchFileInternal(file, outFolder, outName, outExtension, exportFormat);
            }
        }

        /// <summary>
        /// Search files.
        /// </summary>
        /// <param name="files">The files to search.</param>
        public void SearchFiles(IEnumerable<string> files)
        {
            string exportFormat = GetExportFormat();
            string outExtension = FromAssimpContext.GetFormatExtension(exportFormat);
            foreach (var file in files)
            {
                string folder = PathHandler.GetDirectoryName(file);
                string outFolder = GetOutputFolder(folder);
                string outName = Path.GetFileName(file);
                SearchFileInternal(file, outFolder, outName, outExtension, exportFormat);
            }
        }

        /// <summary>
        /// Search a file.
        /// </summary>
        /// <param name="file">The file to search.</param>
        public void SearchFile(string file)
        {
            string exportFormat = GetExportFormat();
            string folder = PathHandler.GetDirectoryName(file);
            string outFolder = GetOutputFolder(folder);
            string outName = Path.GetFileName(file);
            string outExtension = FromAssimpContext.GetFormatExtension(exportFormat);
            SearchFileInternal(file, outFolder, outName, outExtension, exportFormat);
        }

        #endregion

        #region Search Internal

        private void SearchFileInternal(string file, string outFolder, string outName, string outExtension, string exportFormat)
        {
#if !DEBUG_DISABLE_TRY_CATCH
            try
            {
#endif
                using Stream stream = GetDecompressedStream(File.OpenRead(file));
                if (TryReadBinder(stream, out IBinder? binder))
                {
                    outFolder = PathHandler.Combine(outFolder, outName.Replace('.', '-'));
                    SearchBinder(binder, outFolder, outExtension, exportFormat);
                }
                else if (TryReadSplitBinder(stream, file, TryFindStreamFromPath, out binder))
                {
                    outFolder = PathHandler.Combine(outFolder, outName.Replace('.', '-'));
                    SearchBinder(binder, outFolder, outExtension, exportFormat);
                }
                else if (SearchForZero3 && file.EndsWith(".000") && TryReadZero3(file, out Zero3? zero3))
                {
                    outFolder = PathHandler.Combine(outFolder, outName.Replace('.', '-'));
                    SearchZero3(zero3, outFolder, outExtension, exportFormat);
                }
                else
                {
                    Convert(stream, outFolder, outName, outExtension, exportFormat);
                }
#if !DEBUG_DISABLE_TRY_CATCH
            }
            catch (Exception ex)
            {
                LogError($"Error while searching {outName}:\n{ex.Message}\nStacktrace:\n{ex.StackTrace}");
            }
#endif
        }

        private void SearchBinder(IBinder binder, string outFolder, string outExtension, string exportFormat)
        {
            bool FindSplitBinder(string name, [NotNullWhen(true)] out Stream? result)
                    => TryFindStreamInBinder(binder, name, out result);

            foreach (var file in binder.Files)
            {
                using Stream stream = GetDecompressedStream(file.Bytes);
                if (RecursiveSearch && TryReadBinder(stream, out IBinder? innerBinder))
                {
                    outFolder = PathHandler.Combine(outFolder, file.Name.Replace('.', '-'));
                    SearchBinder(innerBinder, outFolder, outExtension, exportFormat);
                }
                else if (RecursiveSearch && TryReadSplitBinder(stream, file.Name, FindSplitBinder, out innerBinder))
                {
                    outFolder = PathHandler.Combine(outFolder, file.Name.Replace('.', '-'));
                    SearchBinder(innerBinder, outFolder, outExtension, exportFormat);
                }
                else
                {
                    string? folder = Path.GetDirectoryName(file.Name);
                    if (!string.IsNullOrWhiteSpace(folder))
                        outFolder = PathHandler.Combine(outFolder, folder);

                    string outName = Path.GetFileName(file.Name);
                    Convert(stream, outFolder, outName, outExtension, exportFormat);
                }
            }
        }

        private void SearchZero3(Zero3 zero3, string outFolder, string outExtension, string exportFormat)
        {
            bool FindSplitBinder(string name, [NotNullWhen(true)] out Stream? result)
                    => TryFindStreamInZero3(zero3, name, out result);

            foreach (var file in zero3.Files)
            {
                using Stream stream = GetDecompressedStream(file.Bytes);
                if (RecursiveSearch && TryReadBinder(stream, out IBinder? innerBinder))
                {
                    outFolder = PathHandler.Combine(outFolder, file.Name.Replace('.', '-'));
                    SearchBinder(innerBinder, outFolder, outExtension, exportFormat);
                }
                else if (RecursiveSearch && TryReadSplitBinder(stream, file.Name, FindSplitBinder, out innerBinder))
                {
                    outFolder = PathHandler.Combine(outFolder, file.Name.Replace('.', '-'));
                    SearchBinder(innerBinder, outFolder, outExtension, exportFormat);
                }
                else
                {
                    string? folder = Path.GetDirectoryName(file.Name);
                    if (!string.IsNullOrWhiteSpace(folder))
                        outFolder = PathHandler.Combine(outFolder, folder);

                    string outName = Path.GetFileName(file.Name);
                    Convert(stream, outFolder, outName, outExtension, exportFormat);
                }
            }
        }

        #endregion

        #region Convert

        private bool Convert(Stream stream, string outFolder, string outName, string outExtension, string exportFormat)
        {
#if !DEBUG_DISABLE_TRY_CATCH
            try
            {
#endif
                if (SearchForTpf && TPF.IsRead(stream, out TPF tpf))
                {
                    if (!outName.Contains("tpf"))
                        outName += ".tpf";

                    outFolder = PathHandler.Combine(outFolder, outName.Replace('.', '-'));
                    ExportTextures(tpf, outFolder);
                    CopyTexture(stream, outFolder, outName);
                    return true;
                }

                string outFileName = $"{outName}.{outExtension}";
                string outPath = PathHandler.Combine(outFolder, outFileName);
                if (!ReplaceExistingFiles && File.Exists(outPath))
                    return false;

                Scene scene;
                if (SearchForFlver2 && FLVER2.IsRead(stream, out FLVER2 flver2))
                    scene = Context.ImportFileFromFlver2(flver2);
                else if (SearchForFlver0 && FLVER0.IsRead(stream, out FLVER0 flver0))
                    scene = Context.ImportFileFromFlver0(flver0);
                else if (SearchForMdl4 && MDL4.IsRead(stream, out MDL4 mdl4))
                    scene = Context.ImportFileFromMdl4(mdl4);
                else if (SearchForSmd4 && SMD4.IsRead(stream, out SMD4 smd4))
                    scene = Context.ImportFileFromSmd4(smd4);
                else
                    return false;

                string? createFolder = Path.GetDirectoryName(outPath);
                if (string.IsNullOrWhiteSpace(createFolder))
                    createFolder = outFolder;
                Directory.CreateDirectory(createFolder);
                ExportModel(scene, outName, outPath, exportFormat);
                CopyModel(stream, outFolder, outName);
                return true;
#if !DEBUG_DISABLE_TRY_CATCH
            }
            catch (Exception ex)
            {
                LogError($"Error while converting {outName}:\n{ex.Message}\nStacktrace:\n{ex.StackTrace}");
                return false;
            }
#endif
        }

        private void CopyModel(Stream stream, string outFolder, string outName)
        {
            if (CopyImport)
            {
                Directory.CreateDirectory(outFolder);
                string importOutPath = PathHandler.Combine(outFolder, outName);
                if (ReplaceExistingFiles || !File.Exists(importOutPath))
                {
                    CopyImportToExportLocation(stream, importOutPath);
                    LogModelCopied($"Copied: {outName}");
                }
            }
        }

        private void CopyTexture(Stream stream, string outFolder, string outName)
        {
            if (CopyImport)
            {
                Directory.CreateDirectory(outFolder);
                string importOutPath = PathHandler.Combine(outFolder, outName);
                if (ReplaceExistingFiles || !File.Exists(importOutPath))
                {
                    CopyImportToExportLocation(stream, importOutPath);
                    LogTextureCopied($"Copied: {outName}");
                }
            }
        }

        private void ExportModel(Scene scene, string outName, string outPath, string exportFormat)
        {
            // If the root node has the same name as a bone node it confuses things
            // Some files have a single bone with the same name as the file without extensions
            // So for now, keep extensions
            string rootName = outName;
            scene.RootNode.Name = rootName;
            if (FixRootNode && FromAssimpContext.IsFbxFormat(exportFormat))
            {
                // Can't set old root node parent unfortunately...
                var oldRootNode = scene.RootNode;
                var newRootNode = new Node("Root");
                newRootNode.Children.Add(oldRootNode);
                scene.RootNode = newRootNode;
            }

            if (Context.ExportFile(scene, outPath, exportFormat, ExportFlags))
            {
                LogModel($"Converted: {outName}");
            }
            else
            {
                LogModel($"Failed: {outName}");
            }
        }

        private void ExportTextures(TPF tpf, string outFolder)
        {
            if (tpf.Textures.Count > 1)
            {
                Directory.CreateDirectory(outFolder);
            }

            foreach (var texture in tpf.Textures)
            {
                string outName = $"{texture.Name}.dds";
                string outPath = PathHandler.Combine(outFolder, outName);
                if (!ReplaceExistingFiles && File.Exists(outPath))
                    continue;

                string? createFolder = Path.GetDirectoryName(outPath);
                if (!string.IsNullOrWhiteSpace(createFolder))
                    Directory.CreateDirectory(createFolder);

                byte[] bytes;
                if (tpf.Platform == TPF.TPFPlatform.PC)
                {
                    bytes = texture.Bytes;
                }
                else
                {
                    bytes = texture.Headerize();
                }

                File.WriteAllBytes(outPath, bytes);
                LogTexture($"Extracted: {outName}");
            }
        }

        #endregion

        #region Try Read

        private bool TryReadBinder(Stream stream, [NotNullWhen(true)] out IBinder? binder)
        {
            if (SearchForBND3 && BND3.IsRead(stream, out BND3 bnd3))
            {
                binder = bnd3;
                return true;
            }
            else if (SearchForBND4 && BND4.IsRead(stream, out BND4 bnd4))
            {
                binder = bnd4;
                return true;
            }

            binder = null;
            return false;
        }

        private bool TryReadSplitBinder(Stream stream, string name, TryFindStream tryFind, [NotNullWhen(true)] out IBinder? binder)
        {
            if (!SearchForBXF3 && !SearchForBXF4)
            {
                binder = null;
                return false;
            }

            if (name.Contains("bhd"))
            {
                string dataName = name.Replace("bhd", "bdt");
                if (SearchForBXF3 && BXF3.IsHeader(stream))
                {
                    if (tryFind(dataName, out Stream? dataStream)
                        && TryReadBXF3(stream, dataStream, name, out BXF3? result))
                    {
                        binder = result;
                        return true;
                    }
                }
                else if (SearchForBXF4 && BXF4.IsHeader(stream))
                {
                    if (tryFind(dataName, out Stream? dataStream)
                        && TryReadBXF4(stream, dataStream, name, out BXF4? result))
                    {
                        binder = result;
                        return true;
                    }
                }
            }

            binder = null;
            return false;
        }

        private bool TryReadZero3(string path, [NotNullWhen(true)] out Zero3? result)
        {
            try
            {
                result = Zero3.Read(path);
                return true;
            }
            catch
            {
                LogWarning($"Detected potential {nameof(Zero3)} but could not read it: {Path.GetFileName(path)}");
            }

            result = null;
            return false;
        }

        private bool TryReadBXF3(Stream header, Stream data, string name, [NotNullWhen(true)] out BXF3? result)
        {
            try
            {
                result = BXF3.Read(header, data);
                return true;
            }
            catch
            {
                LogWarning($"Detected potential {nameof(BXF3)} but could not read it: {name}");
            }

            result = null;
            return false;
        }

        private bool TryReadBXF4(Stream header, Stream data, string name, [NotNullWhen(true)] out BXF4? result)
        {
            try
            {
                result = BXF4.Read(header, data);
                return true;
            }
            catch
            {
                LogWarning($"Detected potential {nameof(BXF4)} but could not read it: {name}");
            }

            result = null;
            return false;
        }

        #endregion

        #region Try Find

        private static bool TryFindStreamInBinder(IBinder binder, string name, [NotNullWhen(true)] out Stream? result)
        {
            foreach (var file in binder.Files)
            {
                if (file.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = new MemoryStream(file.Bytes, false);
                    return true;
                }
            }

            result = null;
            return false;
        }

        private static bool TryFindStreamInZero3(Zero3 archive, string name, [NotNullWhen(true)] out Stream? result)
        {
            foreach (var file in archive.Files)
            {
                if (file.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = new MemoryStream(file.Bytes, false);
                    return true;
                }
            }

            result = null;
            return false;
        }

        private static bool TryFindStreamFromPath(string path, [NotNullWhen(true)] out Stream? result)
        {
            if (File.Exists(path))
            {
                result = File.OpenRead(path);
                return true;
            }

            result = null;
            return false;
        }

        #endregion

        #region Helpers

        private static Stream GetDecompressedStream(Stream stream)
        {
            if (DCX.Is(stream))
            {
                Stream newStream = new MemoryStream(DCX.Decompress(stream), false);
                stream.Dispose();
                return newStream;
            }

            return stream;
        }

        private static MemoryStream GetDecompressedStream(byte[] bytes)
        {
            if (DCX.Is(bytes))
            {
                return new MemoryStream(DCX.Decompress(bytes), false);
            }

            return new MemoryStream(bytes, false);
        }

        private string GetOutputFolder(string folder)
        {
            string outputFolder;
            if (UseRootFolder && !string.IsNullOrEmpty(RootFolder) && !string.IsNullOrEmpty(RootFolderOverride))
            {
                outputFolder = folder.Replace(RootFolder, RootFolderOverride);
            }
            else
            {
                outputFolder = folder;
            }

            return outputFolder;
        }

        private string GetExportFormat()
        {
            string format = ExportFormat;
            if (!FormatCache.IsSupportedExportFormat(format))
            {
                format = format switch
                {
                    "dae" => "collada",
                    _ => "fbx",
                };
            }

            return format;
        }

        private static void CopyImportToExportLocation(Stream import, string outPath)
        {
            using var fs = File.OpenWrite(outPath);
            import.CopyTo(fs);
        }

        #endregion

        #region Logging Methods

        private void LogModel(string value)
        {
            if (LogModelsExported)
                Logger.WriteLine(value);
        }

        private void LogTexture(string value)
        {
            if (LogTexturesExported)
                Logger.WriteLine(value);
        }

        private void LogModelCopied(string value)
        {
            if (LogModelsCopied)
                Logger.WriteLine(value);
        }

        private void LogTextureCopied(string value)
        {
            if (LogTexturesCopied)
                Logger.WriteLine(value);
        }

        private void LogWarning(string value)
        {
            if (LogWarnings)
                Logger.WriteLine(value);
        }

        private void LogError(string value)
        {
            if (LogErrors)
                Logger.WriteLine(value);
        }

        #endregion

        #region Delegates

        private delegate bool TryFindStream(string name, [NotNullWhen(true)] out Stream? result);

        #endregion
    }
}
