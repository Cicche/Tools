using System;
using System.IO;
using System.Text;
using Tools.Core.Abstractions;

namespace Tools.Core.Services
{
    public class FileLogger : IAppLogger
    {
        private static readonly object Sync = new object();
        private static bool cleanupDone;
        private readonly string logsDir;
        private const int RetentionDays = 30;

        public FileLogger()
        {
            logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        }

        public void Info(string category, string message)
        {
            Write("INFO", category, message);
        }

        public void Warn(string category, string message)
        {
            Write("WARN", category, message);
        }

        public void Error(string category, string message)
        {
            Write("ERROR", category, message);
        }

        private void Write(string level, string category, string message)
        {
            try
            {
                string safeCategory = string.IsNullOrWhiteSpace(category) ? "GENERAL" : category.Trim().ToUpperInvariant();
                string safeMessage = Sanitize(message);
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string line = $"{timestamp} | {level} | {safeCategory} | {safeMessage}";

                lock (Sync)
                {
                    Directory.CreateDirectory(logsDir);
                    EnsureRetention();
                    string filePath = Path.Combine(logsDir, $"tools-{DateTime.Now:yyyyMMdd}.log");
                    File.AppendAllText(filePath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // No throw: il logger non deve interrompere il flusso applicativo.
            }
        }

        private static string Sanitize(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return string.Empty;
            return message.Replace("\r", " ").Replace("\n", " ").Trim();
        }

        private void EnsureRetention()
        {
            if (cleanupDone) return;
            cleanupDone = true;

            try
            {
                DateTime threshold = DateTime.Now.Date.AddDays(-RetentionDays);
                foreach (string file in Directory.GetFiles(logsDir, "tools-*.log"))
                {
                    DateTime lastWrite = File.GetLastWriteTime(file);
                    if (lastWrite < threshold)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch
            {
                // Nessun rilancio: il cleanup non deve bloccare l'applicazione.
            }
        }
    }
}
