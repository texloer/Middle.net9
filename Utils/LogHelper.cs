using log4net;
using System.Runtime.CompilerServices;

namespace Middle.Utils
{
    /// <summary>
    /// 日志帮助类 - 简化日志调用
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// 获取调用者的 Logger
        /// </summary>
        private static ILog GetLogger([CallerFilePath] string callerFilePath = "")
        {
            string className = System.IO.Path.GetFileNameWithoutExtension(callerFilePath);
            return LogManager.GetLogger(className);
        }

        public static void Debug(string message, [CallerFilePath] string callerFilePath = "")
        {
            GetLogger(callerFilePath).Debug(message);
        }

        public static void Info(string message, [CallerFilePath] string callerFilePath = "")
        {
            GetLogger(callerFilePath).Info(message);
        }

        public static void Warn(string message, [CallerFilePath] string callerFilePath = "")
        {
            GetLogger(callerFilePath).Warn(message);
        }

        public static void Error(string message, Exception? ex = null, [CallerFilePath] string callerFilePath = "")
        {
            var logger = GetLogger(callerFilePath);
            if (ex != null)
                logger.Error(message, ex);
            else
                logger.Error(message);
        }

        public static void Fatal(string message, Exception? ex = null, [CallerFilePath] string callerFilePath = "")
        {
            var logger = GetLogger(callerFilePath);
            if (ex != null)
                logger.Fatal(message, ex);
            else
                logger.Fatal(message);
        }
    }
}