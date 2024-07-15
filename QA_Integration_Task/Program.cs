using CommandLine;
using NLog;
using Sync;

namespace QA_Integration_Task
{
    class Program
    {
        // Logger instance for logging
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Options class to define command-line arguments
        public class Options
        {
            [Option('s', "source", Required = true, HelpText = "Source folder path.")]
            public string SourceFolder { get; set; }

            [Option('r', "replica", Required = true, HelpText = "Replica folder path.")]
            public string ReplicaFolder { get; set; }

            [Option('i', "interval", Required = true, HelpText = "Synchronization interval in seconds.")]
            public int SyncInterval { get; set; }

            [Option('l', "log", Required = true, HelpText = "Log file path.")]
            public string LogPath { get; set; }
               
        }

        // Main method, the entry point of the application
        static void Main(string[] args)
        {
            // Parse the command line arguments
            var result = Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }

        // Method to run the synchronization process if arguments are parsed successfully
        static void RunOptionsAndReturnExitCode(Options opts)
        {
            // Load the NLog configuration from the specified log file path
            LogManager.LoadConfiguration(opts.LogPath);
            // Create an instance of FolderSynchronizer and start the synchronization
            var synchronizer = new FolderSynchronizer(opts.SourceFolder, 
                opts.ReplicaFolder, opts.SyncInterval);
            synchronizer.Start();
        }

        // Method to handle errors during argument parsing
        static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var err in errs)
            {
                Console.WriteLine(err.ToString());
            }
        }
    }
}

