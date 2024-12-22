#if DEBUG
//#define DEBUG_DISABLE_TRY_CATCH
//#define TEST_MODE
#endif

using MassConvertFromModel.Handlers;
using FromAssimp;
using System.Diagnostics;
using MassConvertFromModel.Loggers;
using MassConvertFromModel.Configs;

namespace MassConvertFromModel
{
    /// <summary>
    /// The main program class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Gets the folder this program is in.
        /// </summary>
        /// <returns>The folder this program is in if found.</returns>
        private static string? GetAppFolder()
        {
            string executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(executablePath);
        }

        /// <summary>
        /// Gets the config from the app folder if present.
        /// </summary>
        /// <param name="appFolder">The folder this program is in.</param>
        /// <returns>The read config or default config.</returns>
        private static ConverterConfig GetConfig(string? appFolder)
        {
            var config = new ConverterConfig();
            if (string.IsNullOrWhiteSpace(appFolder))
            {
                Console.WriteLine("Warning: Could not get the name of the folder this program is in to check config, will use default config.");
            }
            else
            {
                Console.WriteLine("Parsing config...");
                string configPath = PathHandler.Combine(appFolder, "config.txt");
                PathHandler.EnsureFileExists(configPath);
                config.Parse(configPath);

                string assimpFlagsPath = PathHandler.Combine(appFolder, "assimpflags.txt");
                PathHandler.EnsureFileExists(assimpFlagsPath);
                config.ExportFlags = AssimpFlagsParser.Parse(assimpFlagsPath);
            }

            return config;
        }

        /// <summary>
        /// Gets a text writer for a log file if possible.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="appFolder">The folder this program is in.</param>
        /// <returns></returns>
        private static StreamWriter? GetTextWriter(ConverterConfig config, string? appFolder)
        {
            StreamWriter? textWriter = null;
            if (!string.IsNullOrWhiteSpace(appFolder))
            {
                var options = new FileStreamOptions
                {
                    Mode = FileMode.Append,
                    Access = FileAccess.Write,
                    Share = FileShare.Read
                };

                string logPath = Path.Combine(appFolder, "converter.log");
                textWriter = new StreamWriter(logPath, options);
                textWriter.WriteLine($"[File Log Started: {DateTime.Now:MM-dd-yyyy-hh:mm:ss}]");
            }

            return textWriter;
        }

        /// <summary>
        /// Sets up a timed logger.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="textWriter">A text writer for a log file if applicable.</param>
        /// <param name="interval">The interval the logger should update on.</param>
        /// <returns>A timed logger.</returns>
        private static TimedLogger GetTimedLogger(ConverterConfig config, StreamWriter? textWriter, double interval)
        {
            var logger = new TimedLogger(interval);
            if (config.OutputToConsole)
            {
                logger.AddAction(Console.Write);
            }

            if (config.OutputToLog && textWriter != null)
            {
                logger.AddAction(textWriter.Write);
            }

#if DEBUG
            logger.AddAction((string value) => Debug.Write(value));
#endif

            return logger;
        }

        /// <summary>
        /// Sets the options in the converter and assimp context from the config.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <param name="context">The assimp context.</param>
        /// <param name="config">The config.</param>
        private static void SetConfigOptions(SearchingConverter converter, FromAssimpContext context, ConverterConfig config)
        {
            // Converter
            converter.SearchForBND3 = config.SearchBND3;
            converter.SearchForBND4 = config.SearchBND4;
            converter.SearchForBXF3 = config.SearchBND3; // TODO
            converter.SearchForBXF4 = config.SearchBND4; // TODO
            converter.SearchForZero3 = config.SearchZero3;
            converter.SearchForSmd4 = config.SearchForSMD4;
            converter.SearchForMdl4 = config.SearchForMDL4;
            converter.SearchForFlver0 = config.SearchForFlver0;
            converter.SearchForFlver2 = config.SearchForFlver2;
            converter.SearchForTpf = config.SearchForTextures;
            converter.RecursiveSearch = config.BinderRecursiveSearch;
            converter.ReplaceExistingFiles = config.ReplaceExistingFiles;
            converter.LogModelsExported = true; // TODO
            converter.LogTexturesExported = config.OutputTexturesFound;
            converter.LogModelsCopied = true; // TODO
            converter.LogTexturesCopied = true; // TODO
            converter.LogWarnings = true; // TODO
            converter.ExportFormat = config.ExportFormat;
            converter.ExportFlags = config.ExportFlags;

            // Context
            context.DoCheckFlip = config.DoCheckFlip;
            context.ScalelessBones = config.ScalelessBones;
            context.PreferUnitSystemProperty = config.PreferUnitSystemProperty;
            context.ConvertUnitSystem = config.ConvertUnitSystem;
            context.MirrorX = config.MirrorX;
            context.MirrorY = config.MirrorY;
            context.MirrorZ = config.MirrorZ;
            context.ExportScale = config.Scale;
        }

        /// <summary>
        /// Runs a search for things to convert.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <param name="paths">The paths to search.</param>
        private static void RunSearch(SearchingConverter converter, string[] paths)
        {
            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    converter.SearchFolder(path);
                }
                else if (File.Exists(path))
                {
                    converter.SearchFile(path);
                }
            }
        }

        /// <summary>
        /// Runs a test search for things to convert.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <param name="paths">The paths to search.</param>
        private static void RunTestSearch(SearchingConverter converter, string[] paths)
        {
            for (int i = 2; i < paths.Length; i++)
            {
                string path = paths[i];
                if (Directory.Exists(path))
                {
                    converter.SearchFolder(path);
                }
                else if (File.Exists(path))
                {
                    converter.SearchFile(path);
                }
            }
        }

        /// <summary>
        /// The main processing function.
        /// </summary>
        /// <param name="args">The arguments to process.</param>
        static void Run(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("This program does not have a UI, please drag and drop files or folders onto it.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Initialization...");
            string? appFolder = GetAppFolder();
            ConverterConfig config = GetConfig(appFolder);
            var textWriter = GetTextWriter(config, appFolder);
            var logUpdateInterval = 2150d;
            using var logger = GetTimedLogger(config, textWriter, logUpdateInterval);
            using var context = new FromAssimpContext();
            var converter = new SearchingConverter(context, logger, true);
            SetConfigOptions(converter, context, config);

#if TEST_MODE
            Console.WriteLine("Setting up test mode...");
            if (args.Length > 1)
            {
                converter.CopyImport = false;
                converter.UseRootFolder = true;
                converter.RootFolder = args[0];
                converter.RootFolderOverride = args[1];
                Console.WriteLine($"Root folder: {converter.RootFolder}");
                Console.WriteLine($"Root folder override: {converter.RootFolderOverride}");

                Console.WriteLine("Testing...");
                logger.StartTimer();
                RunTestSearch(converter, args);
            }
            else
            {
                Console.WriteLine("Not enough arguments to setup test mode.");
            }
#else
            Console.WriteLine("Searching...");
            logger.StartTimer();
            RunSearch(converter, args);
#endif
            logger.Flush();

            if (textWriter != null)
            {
                textWriter.WriteLine($"[File Log Ended: {DateTime.Now:MM-dd-yyyy-hh:mm:ss}]");
                textWriter.WriteLine();
                textWriter.Dispose();
            }

            logger.Dispose();

            Console.WriteLine("Finished searching.");
            Console.ReadLine();
        }

        /// <summary>
        /// The entry point, used to wrap a try catch around processing.
        /// </summary>
        /// <param name="args">The arguments to process.</param>
        static void Main(string[] args)
        {
#if !DEBUG_DISABLE_TRY_CATCH
            try
            {
#endif
                Run(args);
#if !DEBUG_DISABLE_TRY_CATCH
            }
            catch (Exception ex)
            {
                string error = $"Error in main program:\n{ex.Message}\nStacktrace:\n{ex.StackTrace}";
                Console.WriteLine(error);
                Debug.WriteLine(error);
            }
#endif
        }
    }
}