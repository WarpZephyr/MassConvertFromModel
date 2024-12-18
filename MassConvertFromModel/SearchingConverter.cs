﻿using Assimp;
using FromAssimp;
using MassConvertFromModel.Handlers;
using SoulsFormats;
using System.Diagnostics.CodeAnalysis;

namespace MassConvertFromModel
{
    /// <summary>
    /// Searches through files and archives recursively to convert them.
    /// </summary>
    public class SearchingConverter : IDisposable
    {
        /// <summary>
        /// A log containing logged events according to the config.
        /// </summary>
        public List<string> Log = [];

        /// <summary>
        /// A configuration for the converter.
        /// </summary>
        public SearchingConverterConfig Config = new SearchingConverterConfig();

        /// <summary>
        /// The assimp context converter.
        /// </summary>
        public FromAssimpContext Context = new FromAssimpContext();

        /// <summary>
        /// The chosen delegate for writing a line to output.
        /// </summary>
        public Action<string> WriteLine = Console.WriteLine;

        /// <summary>
        /// Whether or not the converter has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Sets the assimp context options from the config.
        /// </summary>
        public void SetContextOptions()
        {
            // This doesn't feel ideal.
            // Might need to refactor later.
            Context.DoCheckFlip = Config.DoCheckFlip;
            Context.ConvertUnitSystem = Config.ConvertUnitSystem;
            Context.PreferUnitSystemProperty = Config.PreferUnitSystemProperty;
            Context.MirrorX = Config.MirrorX;
            Context.MirrorY = Config.MirrorY;
            Context.MirrorZ = Config.MirrorZ;
            Context.ImportScale = Config.Scale;
        }

        /// <summary>
        /// Search a folder recursively.
        /// </summary>
        /// <param name="path">The path to the folder.</param>
        public void SearchFolder(string path)
        {
            var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                SearchFile(file);
            }
        }

        /// <summary>
        /// Search a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        public void SearchFile(string path)
        {
            string folder = PathHandler.GetDirectoryName(path);

            if (Config.SearchZero3 && path.EndsWith(".000"))
            {
                Zero3? zero3 = null;
                try
                {
                    zero3 = Zero3.Read(path);
                }
                catch
                {
                    Output($"Detected potential Zero3 but it could not be read: {Path.GetFileName(path)}\n" +
                        $"Attempting any other searches.");
                }

                if (zero3 != null)
                {
                    SearchZero3(zero3, PathHandler.Combine(folder, PathHandler.GetWithoutExtensions(path)));
                    return;
                }
            }
            
            if (Config.SearchBND3 && BND3.IsRead(path, out BND3 bnd3))
            {
                SearchBinder(bnd3, PathHandler.Combine(folder, PathHandler.GetWithoutExtensions(path)));
            }
            else if (Config.SearchBND4 && BND4.IsRead(path, out BND4 bnd4))
            {
                SearchBinder(bnd4, PathHandler.Combine(folder, PathHandler.GetWithoutExtensions(path)));
            }
            else
            {
                var fileInfo = new FileInfo(path);
                if (fileInfo.Length >= int.MaxValue)
                {
                    Console.WriteLine($"Skipping {Path.GetFileName(path)} because it is too large to read.");
                    return;
                }

                Convert(File.ReadAllBytes(path), Path.GetFileName(path), folder);
            }
        }

        /// <summary>
        /// Search a binder recursively.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="outFolder">The extract path for potential binder extraction.</param>
        void SearchBinder(IBinder binder, string outFolder)
        {
            foreach (var file in binder.Files)
            {
                if (Config.BinderRecursiveSearch && Config.SearchBND3 && BND3.IsRead(file.Bytes, out BND3 bnd3))
                {
                    SearchBinder(bnd3, PathHandler.Combine(outFolder, PathHandler.GetWithoutExtensions(file.Name)));
                }
                else if (Config.BinderRecursiveSearch && Config.SearchBND4 && BND4.IsRead(file.Bytes, out BND4 bnd4))
                {
                    SearchBinder(bnd4, PathHandler.Combine(outFolder, PathHandler.GetWithoutExtensions(file.Name)));
                }
                else
                {
                    Convert(file.Bytes, Path.GetFileName(file.Name), PathHandler.Combine(outFolder, PathHandler.GetDirectoryName(file.Name)));
                }
            }
        }

        /// <summary>
        /// Search a multi container archive from Armored Core 4 recursively.
        /// </summary>
        /// <param name="zero3">The archive.</param>
        /// <param name="outFolder">The extract path for potential extraction.</param>
        void SearchZero3(Zero3 zero3, string outFolder)
        {
            foreach (var file in zero3.Files)
            {
                if (Config.SearchBND3 && BND3.IsRead(file.Bytes, out BND3 bnd3))
                {
                    SearchBinder(bnd3, PathHandler.Combine(outFolder, PathHandler.GetWithoutExtensions(file.Name)));
                }
                else if (Config.SearchBND4 && BND4.IsRead(file.Bytes, out BND4 bnd4))
                {
                    SearchBinder(bnd4, PathHandler.Combine(outFolder, PathHandler.GetWithoutExtensions(file.Name)));
                }
                else
                {
                    Convert(file.Bytes, Path.GetFileName(file.Name), PathHandler.Combine(outFolder, PathHandler.GetDirectoryName(file.Name)));
                }
            }
        }

        /// <summary>
        /// Convert a file if a supported match is found.
        /// </summary>
        /// <param name="bytes">The bytes of the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="outFolder">The folder to output the converted file to.</param>
        void Convert(byte[] bytes, string fileName, string outFolder)
        {
#if !DEBUG
            try
            {
#endif
            outFolder = PathHandler.GetFolderOnlyPath(outFolder);
            string outName = $"{fileName}.{FromAssimpContext.GetFormatExtension(Config.ExportFormat)}";
            string outPath = PathHandler.Combine(outFolder, outName);

            if (!Config.ReplaceExistingFiles && File.Exists(outPath))
            {
                Output($"Skipping {fileName}");
                return;
            }

            if ((Config.SearchForFlver2 && TryFlver2(bytes, out Scene? scene))
                || (Config.SearchForFlver0 && TryFlver0(bytes, out scene))
                || (Config.SearchForMDL4 && TryMdl4(bytes, out scene))
                || (Config.SearchForSMD4 && TrySmd4(bytes, out scene)))
            {
                Directory.CreateDirectory(outFolder);

                // If the root node has the same name as a bone node it confuses things
                // Some files have a single bone with the same name as the file without extensions
                // So for now, keep extensions
                string rootName = fileName;
                scene.RootNode.Name = rootName;
                if (Config.FixRootNode && FromAssimpContext.IsFbxFormat(Config.ExportFormat))
                {
                    // Can't set old root node parent unfortunately...
                    var oldRootNode = scene.RootNode;
                    var newRootNode = new Node("Root");
                    newRootNode.Children.Add(oldRootNode);
                    scene.RootNode = newRootNode;
                }

                if (Context.ExportFile(scene, outPath, Config.ExportFormat, Config.ExportFlags))
                {
                    Output($"Converted: {fileName}");
                }
                else
                {
                    Output($"Failed: {fileName}");
                }
            }
            else if (Config.SearchForTextures)
            {
                TryTpf(bytes, fileName, outFolder);
            }
#if !DEBUG
            }
            catch (Exception ex)
            {
                Output($"Error Converting {fileName}: {ex.Message}");
            }
#endif
        }

        /// <summary>
        /// Try to convert data as a <see cref="FLVER0"/>.
        /// </summary>
        /// <param name="bytes">The raw bytes of the data.</param>
        /// <returns>Whether or not the data was read as a <see cref="FLVER0"/> and converted.</returns>
        bool TryFlver0(byte[] bytes, [NotNullWhen(true)] out Scene? scene)
        {
            if (FLVER0.IsRead(bytes, out FLVER0 model))
            {
                scene = Context.ImportFileFromFlver0(model);
                return true;
            }

            scene = null;
            return false;
        }

        /// <summary>
        /// Try to convert data as a <see cref="FLVER2"/>.
        /// </summary>
        /// <param name="bytes">The raw bytes of the data.</param>
        /// <returns>Whether or not the data was read as a <see cref="FLVER2"/> and converted.</returns>
        bool TryFlver2(byte[] bytes, [NotNullWhen(true)] out Scene? scene)
        {
            if (FLVER2.IsRead(bytes, out FLVER2 model))
            {
                scene = Context.ImportFileFromFlver2(model);
                return true;
            }

            scene = null;
            return false;
        }

        /// <summary>
        /// Try to convert data as a <see cref="MDL4"/>.
        /// </summary>
        /// <param name="bytes">The raw bytes of the data.</param>
        /// <returns>Whether or not the data was read as a <see cref="MDL4"/> and converted.</returns>
        bool TryMdl4(byte[] bytes, [NotNullWhen(true)] out Scene? scene)
        {
            if (MDL4.IsRead(bytes, out MDL4 model))
            {
                scene = Context.ImportFileFromMdl4(model);
                return true;
            }

            scene = null;
            return false;
        }

        /// <summary>
        /// Try to convert data as a <see cref="SMD4"/>.
        /// </summary>
        /// <param name="bytes">The raw bytes of the data.</param>
        /// <returns>Whether or not the data was read as a <see cref="SMD4"/> and converted.</returns>
        bool TrySmd4(byte[] bytes, [NotNullWhen(true)] out Scene? scene)
        {
            if (SMD4.IsRead(bytes, out SMD4 model))
            {
                scene = Context.ImportFileFromSmd4(model);
                return true;
            }

            scene = null;
            return false;
        }

        /// <summary>
        /// Try to extract data as a <see cref="TPF"/>.
        /// </summary>
        /// <param name="bytes">The raw bytes of the data.</param>
        /// <param name="fileName">The name of the file containing the data.</param>
        /// <param name="outFolder">The folder to output extracted <see cref="TPF"/> data to.</param>
        /// <returns>Whether or not the data was read as a <see cref="TPF"/> and extracted.</returns>
        bool TryTpf(byte[] bytes, string fileName, string outFolder)
        {
            if (TPF.IsRead(bytes, out TPF tpf))
            {
                string outFolderName = fileName.Replace(".", "-");
                if (!outFolderName.EndsWith("-tpf"))
                    outFolderName += "-tpf";

                outFolder = PathHandler.Combine(outFolder, outFolderName);
                outFolder = PathHandler.GetFolderOnlyPath(outFolder);
                Directory.CreateDirectory(outFolder);
                foreach (var texture in tpf.Textures)
                {
                    string outPath = PathHandler.Combine(outFolder, $"{texture.Name}.dds");
                    if (tpf.Platform != TPF.TPFPlatform.PC)
                    {
                        File.WriteAllBytes(outPath, texture.Bytes);
                    }
                    else
                    {
                        File.WriteAllBytes(outPath, texture.Headerize());
                    }
                }

                if (Config.OutputTexturesFound)
                {
                    Output($"Extracted TPF {PathHandler.GetWithoutExtensions(fileName)}");
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Output something to valid logging locations.
        /// </summary>
        /// <param name="value">The value to output.</param>
        void Output(string value)
        {
            if (Config.OutputToConsole)
            {
                WriteLine(value);
            }

            if (Config.OutputToLog)
            {
                Log.Add(value);
            }
        }

        /// <summary>
        /// Write the log to a file in the provided folder.
        /// </summary>
        /// <param name="folder">The folder to write the log to.</param>
        public void WriteLogToFolder(string folder)
            => File.WriteAllLines(PathHandler.Combine(folder, $"log-{DateTime.Now:MMddyyyy-hhmmss}.txt"), Log);

        /// <summary>
        /// Write the log to the provided path.
        /// </summary>
        /// <param name="path">The path to write the log to.</param>
        public void WriteLogToPath(string path)
            => File.WriteAllLines(path, Log);

        /// <summary>
        /// Disposes the converter.
        /// </summary>
        /// <param name="disposing">Whether or not to dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }

                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
