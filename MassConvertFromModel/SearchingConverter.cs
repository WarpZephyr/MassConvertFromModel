﻿using SoulsFormats;

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

            if (BND3.IsRead(path, out BND3 bnd3))
            {
                SearchBinder(bnd3, $"{folder}\\{PathHandler.GetExtensionless(path)}");
            }
            else if (BND4.IsRead(path, out BND4 bnd4))
            {
                SearchBinder(bnd4, $"{folder}\\{PathHandler.GetExtensionless(path)}");
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
                if (BND3.IsRead(file.Bytes, out BND3 bnd3))
                {
                    SearchBinder(bnd3, $"{outFolder}\\{PathHandler.GetExtensionless(file.Name)}");
                }
                else if (BND4.IsRead(file.Bytes, out BND4 bnd4))
                {
                    SearchBinder(bnd4, $"{outFolder}\\{PathHandler.GetExtensionless(file.Name)}");
                }
                else
                {
                    Convert(file.Bytes, file.Name, outFolder);
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
            try
            {
                string outPath = $"{outFolder}\\{fileName}.fbx";
                if (File.Exists(outPath) && !Config.ReplaceExistingFiles)
                {
                    Output($"Skipping {fileName}");
                    return;
                }

                if (TryFlver2(bytes, fileName, outFolder, outPath)) return;
                else if (TryFlver0(bytes, fileName, outFolder, outPath)) return;
                else if (TryMdl4(bytes, fileName, outFolder, outPath)) return;
                else if (TrySmd4(bytes, fileName, outFolder, outPath)) return;
            }
            catch (Exception ex)
            {
                Output($"Error Converting {fileName}: \"{ex.Message}\" \"{ex.StackTrace}\"");
            }
        }

        bool TryFlver2(byte[] bytes, string fileName, string outFolder, string outPath)
        {
            if (FLVER2.IsRead(bytes, out FLVER2 model))
            {
                if (AssimpExport.ExportModel(model, outFolder, outPath, "fbx"))
                {
                    Output($"Converted {fileName}");
                }
                else
                {
                    Output($"Failed Converting {fileName}");
                }
                return true;
            }
            return false;
        }

        bool TryFlver0(byte[] bytes, string fileName, string outFolder, string outPath)
        {
            if (FLVER0.IsRead(bytes, out FLVER0 model))
            {
                if (AssimpExport.ExportModel(model, outFolder, outPath, "fbx"))
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
                if (AssimpExport.ExportModel(model, outFolder, outPath, "fbx"))
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
                if (AssimpExport.ExportModel(model, outFolder, outPath, "fbx"))
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
            File.WriteAllLines($"{folder}\\log-{DateTime.Now:MMddyyyy-hhmmss}.txt", Log);
        }
    }
}
