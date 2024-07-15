using NLog;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Sync
{
    public class FolderSynchronizer
    {
        private readonly string sourceFolder;
        private readonly string replicaFolder;
        private readonly int syncInterval;
        private readonly CancellationTokenSource cancellationTokenSource;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public FolderSynchronizer(string source, string replica, int interval)
        {
            this.sourceFolder = source;
            this.replicaFolder = replica;
            this.syncInterval = interval;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            try
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    SyncFolders();
                    Console.WriteLine("Synchronization complete. Next sync in {0} seconds...", syncInterval);
                    Thread.Sleep(syncInterval * 1000);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Info("Synchronization cancelled.");
                Console.WriteLine("Synchronization cancelled.");
            }
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        private void SyncFolders()
        {
            try
            {
                // Verify that the source folder exists before attempting to get files
                if (!Directory.Exists(sourceFolder))
                {
                    Logger.Error($"Source folder does not exist: {sourceFolder}");
                    Console.WriteLine($"Source folder does not exist: {sourceFolder}");
                    return;
                }

                var sourceFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
                var replicaFiles = Directory.GetFiles(replicaFolder, "*.*", SearchOption.AllDirectories);

                // Copy or update files from source to replica
                foreach (var sourceFile in sourceFiles)
                {
                    var relativePath = sourceFile.Substring(sourceFolder.Length + 1);
                    var replicaFile = Path.Combine(replicaFolder, relativePath);

                    if (!File.Exists(replicaFile) || !FileComparer.FilesAreEqual(sourceFile, replicaFile))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(replicaFile));
                        File.Copy(sourceFile, replicaFile, true);
                        Logger.Info($"Copied: {sourceFile} to {replicaFile}");
                        Console.WriteLine($"Copied: {sourceFile} to {replicaFile}");
                    }
                }

                // Delete files in replica that are not in source
                foreach (var replicaFile in replicaFiles)
                {
                    var relativePath = replicaFile.Substring(replicaFolder.Length + 1);
                    var sourceFile = Path.Combine(sourceFolder, relativePath);

                    if (!File.Exists(sourceFile))
                    {
                        File.Delete(replicaFile);
                        Logger.Info($"Deleted: {replicaFile}");
                        Console.WriteLine($"Deleted: {replicaFile}");
                    }
                }

                // Delete empty directories in replica
                var replicaDirs = Directory.GetDirectories(replicaFolder, "*", SearchOption.AllDirectories)
                                           .OrderByDescending(d => d.Length);

                foreach (var dir in replicaDirs)
                {
                    if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
                    {
                        Directory.Delete(dir);
                        Logger.Info($"Deleted empty directory: {dir}");
                        Console.WriteLine($"Deleted empty directory: {dir}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during synchronization");
                Console.WriteLine($"Error during synchronization: {ex.Message}");
            }
        }
    }
}
