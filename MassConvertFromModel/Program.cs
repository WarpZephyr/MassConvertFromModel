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
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Initialization...");
            var textOutputConsole = new TextOutputConsole(bufferThreshold: 50);
            var searcher = new SearchingConverter();
            searcher.WriteLine = textOutputConsole.WriteLine;

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

                string assimpFlagsPath = PathHandler.Combine(folder, "assimpflags.txt");
                PathHandler.EnsureFileExists(assimpFlagsPath);
                searcher.Config.ExportFlags = AssimpFlagsParser.Parse(assimpFlagsPath);
            }

            searcher.SetContextOptions();

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
            textOutputConsole.Finish();

            if (searcher.Config.OutputToLog && folder != null)
            {
                searcher.WriteLogToFolder(folder);
            }

            Console.WriteLine("Finished searching.");
            Console.ReadLine();
        }
    }
}