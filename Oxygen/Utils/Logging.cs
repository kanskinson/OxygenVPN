using System;
using System.Drawing;
using System.IO;
using OxygenVPN.Models;

namespace OxygenVPN.Utils {
    public static class Logging {
        public const string LogFile = "logging\\application.log";

        /// <summary>
        ///     信息
        /// </summary>
        /// <param name="text">内容</param>
        public static void Info(string text) {
            Write(text, LogLevel.INFO);
        }

        /// <summary>
        ///     信息
        /// </summary>
        /// <param name="text">内容</param>
        public static void Warning(string text) {
            Write(text, LogLevel.WARNING);
        }

        /// <summary>
        ///     错误
        /// </summary>
        /// <param name="text">内容</param>
        public static void Error(string text) {
            Write(text, LogLevel.ERROR);
        }


        static LoggingForm loggingFormInstance;
        public static void ShowLogForm() {
            if (loggingFormInstance == null || loggingFormInstance.IsDisposed) {
                loggingFormInstance = new LoggingForm();
            }
            if (loggingFormInstance.Visible == false)
                loggingFormInstance.Show();

            loggingFormInstance.SetText(ReadAllLogs());
        }

        public static string ReadAllLogs() {
            string content = string.Empty;
            lock (FileLock) {
                content = File.ReadAllText(LogFile);
            }
            return content;
        }

        private static readonly object FileLock = new object();

        private static void Write(string text, LogLevel logLevel) {
            string line = $@"[{DateTime.Now}][{logLevel.ToString()}] {text}{Global.EOF}";
            lock (FileLock) {
                File.AppendAllText(LogFile, line);
            }
            if (loggingFormInstance != null && loggingFormInstance.CanLog) {

                loggingFormInstance.AppendText(line);
            }
        }

    }
}