using System;
using System.IO;

namespace RobloxServer
{
    public enum LogType
    {
        Backend = 1,
        Success = 2,
        Error = 3,
        Security = 4
    }

    /// <summary>
    /// Replacement for structure/base/logging.js. Writes to the console (visible in xsp4 / IIS Express)
    /// and to App_Data/Logs/yyyy-MM-dd.log.
    /// </summary>
    public static class Logging
    {
        static readonly object Sync = new object();

        public static void Log(LogType type, string message)
        {
            string line = string.Format("{0:yyyy-MM-dd HH:mm:ss} [{1}]: {2}", DateTime.Now, type, message);

            lock (Sync)
            {
                try
                {
                    Console.WriteLine(line);
                    string dir = Path.Combine(Config.DataPath, "Logs");
                    Directory.CreateDirectory(dir);
                    File.AppendAllText(Path.Combine(dir, DateTime.Now.ToString("yyyy-MM-dd") + ".log"), line + Environment.NewLine);
                }
                catch (Exception)
                {
                    // Logging must never take the site down.
                }
            }
        }
    }
}
