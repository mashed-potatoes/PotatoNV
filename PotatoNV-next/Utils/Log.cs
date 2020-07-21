using System;
using System.Text;

namespace PotatoNV_next.Utils
{
    public class Log
    {
        public class LogEventArgs : EventArgs
        {
            public string Message { get; set; }
            public Status Status { get; set; }
        }

        public class ProgressEventArgs : EventArgs
        {
            public int? Value { get; set; }
            public int? MaxValue { get; set; }
            public bool ShowBar { get; set; }
        }

        public static bool PrintDebug { get; set; } = false;
        private static StringBuilder builder = new StringBuilder();

        public delegate void LogHandler(LogEventArgs logEventArgs);
        public static event LogHandler Notify;

        public delegate void ProgressHandler(ProgressEventArgs progressEventArgs);
        public static event ProgressHandler OnProgress;

        public enum Status
        {
            Success,
            Error,
            Info,
            Debug
        }

        private static void AppendToLog(Status status, string message)
        {
            message = message
                .Replace('\n', ' ')
                .Trim();

            var buffer = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {status.ToString().ToUpper(),-8} | {message}";

            builder.AppendLine(buffer);

            if (status == Status.Debug)
            {
                if (!PrintDebug)
                {
                    return;
                }

                message = $"[DEBUG] {message}";
            }

            var args = new LogEventArgs
            {
                Status = status,
                Message = message + Environment.NewLine
            };

            Notify?.Invoke(args);
        }

        public static void Debug(string message)
        {
            AppendToLog(Status.Debug, message);
        }

        public static void Info(string message)
        {
            AppendToLog(Status.Info, message);
        }

        public static void Success(string message)
        {
            AppendToLog(Status.Success, message);
        }

        public static void Error(string message)
        {
            AppendToLog(Status.Error, message);
        }

        public static void SetProgressBar(bool show)
        {
            OnProgress?.Invoke(new ProgressEventArgs
            {
                ShowBar = show,
                MaxValue = 1,
                Value = 0
            });
        }

        public static void SetProgressBar(int value, int max)
        {
            OnProgress?.Invoke(new ProgressEventArgs
            {
                ShowBar = true,
                MaxValue = max,
                Value = value
            });
        }

        public static string GetLog()
        {
            return builder.ToString();
        }
    }
}
