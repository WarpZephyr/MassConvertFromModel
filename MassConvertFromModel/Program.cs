using SoulsFormats;

namespace MassConvertFromModel
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                foreach (string path in args)
                {
                    if (Directory.Exists(path))
                    {
                        ConvertFolder(path);
                    }
                    else if (File.Exists(path))
                    {
                        ConvertFile(path);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in {nameof(Main)}: \"{ex.Message}\"");
            }

            Console.WriteLine("\nFinished converting.");
            Pause();
        }

        static void ConvertFolder(string folderPath)
        {
            try
            {
                var folders = Directory.EnumerateDirectories(folderPath);
                foreach (string folder in folders)
                {
                    ConvertFolder(folder);
                }

                var files = Directory.EnumerateFiles(folderPath);
                foreach (string path in files)
                {
                    ConvertFile(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in {nameof(ConvertFolder)}: \"{ex.Message}\" for folder {folderPath}");
            }
        }

        static void ConvertFile(string path)
        {
            try
            {
                string folder = $"{Path.GetDirectoryName(path)}";

                if (BND3.Is(path))
                {
                    ConvertBinder(BND3.Read(path), $"{folder}\\{GetExtensionless(path)}-BND3");
                }
                else if (BND4.Is(path))
                {
                    ConvertBinder(BND4.Read(path), $"{folder}\\{GetExtensionless(path)}-BND4");
                }
                else if (Supported(path))
                {
                    Convert(File.ReadAllBytes(path), Path.GetFileName(path), folder);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in {nameof(ConvertFile)}: \"{ex.Message}\" for {path}");
            }
        } 

        static void ConvertBinder(IBinder binder, string outFolder)
        {
            try
            {
                foreach (var file in binder.Files)
                {
                    if (Supported(file.Bytes))
                    {
                        Convert(file.Bytes, file.Name, outFolder);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in {nameof(ConvertBinder)}: \"{ex.Message}\"");
            }
        }

        static void Convert(byte[] bytes, string fileName, string outFolder)
        {
            try
            {
                bool success = AssimpExport.ExportModel(bytes, outFolder, $"{outFolder}\\{fileName}.fbx", "fbx");
                if (success == true)
                {
                    Console.WriteLine($"Converted {fileName}");
                }
                if (success == false)
                {
                    Console.WriteLine($"Failed to convert {fileName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while converting {fileName}: \"{ex.Message}\"");
            }
        }

        static string GetExtensionless(string name)
        {
            string extensionlessName = $"{Path.GetFileNameWithoutExtension(name)}";
            while (extensionlessName.Contains('.'))
                extensionlessName = $"{Path.GetFileNameWithoutExtension(extensionlessName)}";
            return extensionlessName;
        }

        static bool Supported(string path)
        {
            if (FLVER2.Is(path)) return true;
            if (FLVER0.Is(path)) return true;
            if (MDL4.Is(path)) return true;
            if (SMD4.Is(path)) return true;
            return false;
        }

        static bool Supported(byte[] bytes)
        {
            if (FLVER2.Is(bytes)) return true;
            if (FLVER0.Is(bytes)) return true;
            if (MDL4.Is(bytes)) return true;
            if (SMD4.Is(bytes)) return true;
            return false;
        }

        static void Pause() => Console.ReadLine();
    }
}