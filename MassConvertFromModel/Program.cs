using MassConvertFromModel.Handlers;

namespace MassConvertFromModel
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("This program does not have a UI, please drag and drop files or folders onto it.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Initialization...");
            var searcher = new SearchingConverter();

            string executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string? folder = Path.GetDirectoryName(executablePath);

            if (folder == string.Empty)
            {
                Console.WriteLine("Error: Could not get the name of the folder this program is in to check config.\nUsing default config.");
            }
            else
            {
                folder ??= executablePath;
                Console.WriteLine("Parsing config...");
                string configPath = PathHandler.Combine(folder, "config.txt");
                PathHandler.EnsureFileExists(configPath);
                searcher.Config.Parse(configPath);
            }

            Console.WriteLine("Searching...");
            foreach (string path in args)
            {
                if (Directory.Exists(path))
                {
                    searcher.SearchFolder(path);
                }
                else if (File.Exists(path))
                {
                    searcher.SearchFile(path);
                }
            }

            if (searcher.Config.OutputToLog && folder != null)
            {
                searcher.WriteLogToFolder(folder);
            }

            Console.WriteLine("Finished searching.");
            Console.ReadLine();
        }
    }
}