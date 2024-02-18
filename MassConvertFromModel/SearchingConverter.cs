using FromAssimp;
using MassConvertFromModel.Handlers;
using SoulsFormats;
using SoulsFormats.AC4;

namespace MassConvertFromModel
{
    public class SearchingConverter
    {
        public List<string> Log = new List<string>();
        public SearchingConverterConfig Config = new SearchingConverterConfig();

        /// <summary>
        /// Search a folder recursively.
        /// </summary>
        /// <param name="path">The path to the folder.</param>
        public void SearchFolder(string path)
        {
            var folders = Directory.EnumerateDirectories(path);
            foreach (string folder in folders)
            {
                SearchFolder(folder);
            }

            var files = Directory.EnumerateFiles(path);
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
            string folder = $"{Path.GetDirectoryName(path)}";

            if (Config.SearchZero3 && Zero3.Is(path))
            {
                Zero3 zero3;
                try
                {
                    zero3 = Zero3.ReadFromPacked(path);
                }
                catch
                {
                    Output($"Detected Zero3 {Path.GetFileName(path)} is not packed, attempting direct read...");
                    zero3 = Zero3.Read(path);
                }

                SearchZero3(zero3, PathHandler.Combine(folder, PathHandler.GetWithoutExtensions(path)));
            }
            else if (Config.SearchBND3 && BND3.IsRead(path, out BND3 bnd3))
            {
                SearchBinder(bnd3, PathHandler.Combine(folder, PathHandler.GetWithoutExtensions(path)));
            }
            if (Config.SearchBND4 && BND4.IsRead(path, out BND4 bnd4))
            {
                SearchBinder(bnd4, PathHandler.Combine(folder, PathHandler.GetWithoutExtensions(path)));
            }
            else
            {
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
            string outPath = PathHandler.Combine(outFolder, $"{fileName}.{FromAssimpContext.GetFormatExtension(Config.ExportFormat)}");
            if (!Config.ReplaceExistingFiles && File.Exists(outPath))
            {
                Output($"Skipping {fileName}");
                return;
            }

            if (Config.SearchForFlver2 && TryFlver2(bytes, fileName, outFolder, outPath)) return;
            else if (Config.SearchForFlver0 && TryFlver0(bytes, fileName, outFolder, outPath)) return;
            else if (Config.SearchForMDL4 && TryMdl4(bytes, fileName, outFolder, outPath)) return;
            else if (Config.SearchForSMD4 && TrySmd4(bytes, fileName, outFolder, outPath)) return;
            else if (Config.SearchForTextures && TPF.IsRead(bytes, out TPF tpf))
            {
                fileName = PathHandler.GetWithoutExtensions(fileName);
                if (!fileName.EndsWith("_t"))
                {
                    fileName += "_t";
                }

                outFolder = PathHandler.Combine(outFolder, fileName);
                Directory.CreateDirectory(outFolder);
                foreach (var texture in tpf.Textures)
                {
                    outPath = PathHandler.Combine(outFolder, $"{texture.Name}.dds");
                    if (tpf.Platform == TPF.TPFPlatform.PC)
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
                    Output($"Found TPF {fileName}");
                }
            }
#if !DEBUG
            }
            catch (Exception ex)
            {
                Output($"Error Converting {fileName}: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    Output(ex.StackTrace);
                }
            }
#endif
        }

        bool TryFlver2(byte[] bytes, string fileName, string outFolder, string outPath)
        {
            if (FLVER2.IsRead(bytes, out FLVER2 model))
            {
                Directory.CreateDirectory(outFolder);
                if (new FromAssimpContext().ImportFileFromFlver2ThenExport(model, outPath, Config.ExportFormat))
                {
                    Output($"Converted {fileName}");
                }
                else
                {
                    Output($"Failed {fileName}");
                }
                return true;
            }
            return false;
        }

        bool TryFlver0(byte[] bytes, string fileName, string outFolder, string outPath)
        {
            if (FLVER0.IsRead(bytes, out FLVER0 model))
            {
                Directory.CreateDirectory(outFolder);
                if (new FromAssimpContext().ImportFileFromFlver0ThenExport(model, outPath, Config.ExportFormat))
                {
                    Output($"Converted {fileName}");
                }
                else
                {
                    Output($"Failed {fileName}");
                }
                return true;
            }
            return false;
        }

        bool TryMdl4(byte[] bytes, string fileName, string outFolder, string outPath)
        {
            if (MDL4.IsRead(bytes, out MDL4 model))
            {
                Directory.CreateDirectory(outFolder);
                if (new FromAssimpContext().ImportFileFromMdl4ThenExport(model, outPath, Config.ExportFormat))
                {
                    Output($"Converted {fileName}");
                }
                else
                {
                    Output($"Failed {fileName}");
                }
                return true;
            }
            return false;
        }

        bool TrySmd4(byte[] bytes, string fileName, string outFolder, string outPath)
        {
            if (SMD4.IsRead(bytes, out SMD4 model))
            {
                Directory.CreateDirectory(outFolder);
                if (new FromAssimpContext().ImportFileFromSmd4ThenExport(model, outPath, Config.ExportFormat))
                {
                    Output($"Converted {fileName}");
                }
                else
                {
                    Output($"Failed {fileName}");
                }
                return true;
            }
            return false;
        }

        void Output(string value)
        {
            if (Config.OutputToConsole)
            {
                Console.WriteLine(value);
            }

            if (Config.OutputToLog)
            {
                Log.Add(value);
            }
        }

        public void WriteLog(string folder)
        {
            File.WriteAllLines(PathHandler.Combine(folder, $"log-{DateTime.Now:MMddyyyy-hhmmss}.txt"), Log);
        }
    }
}
