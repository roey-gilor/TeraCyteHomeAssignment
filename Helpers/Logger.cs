using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeraCyteHomeAssignment.Helpers
{
    /// <summary>
    /// Simple application logger that writes log messages to a local file.
    /// Creates a Logs directory and app.log file automatically if missing.
    /// Ensures logging never crashes the app.
    /// </summary>
    public static class Logger
    {
        // Directory where log files will be stored (e.g., /bin/Logs/)
        private static readonly string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        // Main application log file path (Logs/app.log)
        private static readonly string logFile = Path.Combine(logDir, "app.log");

        /*Static constructor — runs once when Logger is first accessed.
         Creates log directory and file if they do not exist.*/
        static Logger()
        {
            try
            {
                // Create Logs directory if missing
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                // Create Logs directory if missing
                if (!File.Exists(logFile))
                    File.Create(logFile).Dispose();  
                // File.Create needs Dispose() otherwise keeps file handle open
            }
            catch
            {
                // We DO NOT throw here — logger must never crash the app.
                // If logging cannot initialize, app continues without logs.
            }
        }

        public static void Info(string msg) => Write("INFO", msg);
        public static void Warn(string msg) => Write("WARN", msg);
        public static void Error(string msg, Exception ex = null) =>
            Write("ERROR", $"{msg} {ex?.Message}\n{ex?.TargetSite}");

        private static void Write(string level, string message)
        {
            string log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

            try
            {
                // Append log line to log file
                File.AppendAllText(logFile, log + Environment.NewLine);
            }
            catch
            {
                // Silent catch — logging failure should NEVER break the app
                // (e.g., file locked, read-only FS, disk full, etc.)
            }
        }
    }
}
