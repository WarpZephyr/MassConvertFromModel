namespace MassConvertFromModel
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Searching.....");
            var searcher = new SearchingConverter();
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

            string? folder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (folder == null)
            {
                Console.WriteLine("Error: Could not get the name of the folder this program is in to write log.\nPrinting to console instead.");
                foreach (string str in searcher.Log)
                {
                    Console.WriteLine(str);
                }
            }
            else
            {
                searcher.WriteLog(folder);
            }

            Console.WriteLine("Finished searching.");
            Console.ReadLine();
        }
    }
}