using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace FileBackupApp
{
    public enum LogLevel
    {
        Error,
        Info,
        Debug
    }

    public class FileCopySettings
    {
        public List<string> SourceDirectories { get; set; }
        public string TargetDirectory { get; set; }
        public LogLevel LogLevel { get; set; }
    }

    public class Program
    {
        private static string settingsPath = "settings.json";
        private static string logFilePath;

        public static void Main()
        {
            // Read settings from JSON file
            var settings = ReadSettings();

            // Create log file
            logFilePath = "logs\\log_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";

            // Perform file copy
            foreach (var sourceDir in settings.SourceDirectories)
            {
                CopyFiles(sourceDir, settings.TargetDirectory, settings.LogLevel);
            }

            Console.WriteLine("File copy completed.");

            Console.ReadLine();
        }

        private static FileCopySettings ReadSettings()
        {
            if (!File.Exists(settingsPath))
            {
                throw new FileNotFoundException("Settings file not found.");
            }

            var json = File.ReadAllText(settingsPath);
            var settings = JsonConvert.DeserializeObject<FileCopySettings>(json);

            return settings;
        }

        private static void CopyFiles(string sourceDirectory, string targetDirectory, LogLevel logLevel)
        {
            if (!Directory.Exists(sourceDirectory))
            {
                LogMessage(LogLevel.Error, $"Source directory not found: {sourceDirectory}");
                return;
            }

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            try
            {
                var sourceDirectoryName = new DirectoryInfo(sourceDirectory).Name;
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var targetPath = Path.Combine(targetDirectory, $"{sourceDirectoryName}_Backup_{timestamp}");
                Directory.CreateDirectory(targetPath);

                var files = Directory.GetFiles(sourceDirectory);

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var targetFilePath = Path.Combine(targetPath, fileName);
                    File.Copy(file, targetFilePath);

                    LogMessage(LogLevel.Debug, $"Copied: {fileName}");
                }

                // Zip the target directory
                var zipFileName = $"{targetPath}.zip";
                ZipDirectory(targetPath, zipFileName);

                LogMessage(LogLevel.Info, $"Files copied from {sourceDirectory} to {targetPath} and zipped to {zipFileName}");
            }
            catch (Exception ex)
            {
                LogMessage(LogLevel.Error, $"Error copying files: {ex.Message}");
            }
        }

        private static void LogMessage(LogLevel level, string message)
        {
            if (level <= LogLevel.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (level == LogLevel.Debug)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Console.WriteLine($"[{level}] {DateTime.Now} - {message}");
            File.AppendAllText(logFilePath, $"[{level}] {DateTime.Now} - {message}\n");
            Console.ResetColor();
        }

        private static void ZipDirectory(string sourceDirectory, string zipFilePath)
        {
            System.IO.Compression.ZipFile.CreateFromDirectory(sourceDirectory, zipFilePath);
            Directory.Delete(sourceDirectory, true);
        }
    }
}
