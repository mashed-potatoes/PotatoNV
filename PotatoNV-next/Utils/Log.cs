using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PotatoNV_next.Utils
{
    public class Log
    {
        public static bool PrintDebug { get; set; } = false;
        private static List<Action<LogEventArgs>> actions = new List<Action<LogEventArgs>>();

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

            foreach (var act in actions)
            {
                act?.Invoke(args);
            }
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

        public static void AttachListener(Action<LogEventArgs> act)
        {
            actions.Add(act);
        }
    }

    public class LogEventArgs : EventArgs
    {
        public string Message { get; set; }
        public Log.Status Status { get; set; }
    }
}

