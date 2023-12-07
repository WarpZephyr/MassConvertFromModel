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
            string? folder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (folder == null)
            {
                Console.WriteLine("Error: Could not get the name of the folder this program is in to check config.\nUsing default config.");
            }
            else
            {
                Console.WriteLine("Parsing config...");
                string configPath = $"{folder}\\config.txt";
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
                searcher.WriteLog(folder);
            }

            Console.WriteLine("Finished searching.");
            Console.ReadLine();
        }
    }
}