using System.IO;

namespace SemiconductorControlSystem.ViewModels
{
    /// <summary>
    /// 日志页面 ViewModel - 处理日志查询和展示
    /// </summary>
    public class LogViewModel : BaseViewModel
    {
        public override string PageName => "log";

        public override void HandleMessage(string action, string? data)
        {
            log.Info($"[Log] 处理消息 - Action: {action}");

            switch (action)
            {
                case "getLogs":
                    HandleGetLogs(data);
                    break;

                case "clearLogs":
                    HandleClearLogs();
                    break;

                default:
                    log.Warn($"[Log] 未识别的操作: {action}");
                    break;
            }
        }

        private void HandleGetLogs(string? data)
        {
            log.Debug("[Log] 获取日志文件内容");

            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                string appLogPath = Path.Combine(logDir, "app.log");

                if (!File.Exists(appLogPath))
                {
                    SendDataToFrontend(new
                    {
                        type = "logsData",
                        page = PageName,
                        logs = Array.Empty<string>(),
                        message = "日志文件不存在",
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                    return;
                }

                // 读取最后 100 行日志
                var lines = File.ReadLines(appLogPath).Reverse().Take(100).Reverse().ToList();

                SendDataToFrontend(new
                {
                    type = "logsData",
                    page = PageName,
                    logs = lines,
                    totalLines = lines.Count,
                    logPath = appLogPath,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                log.Error("[Log] 读取日志失败", ex);
                SendDataToFrontend(new
                {
                    type = "error",
                    page = PageName,
                    message = $"读取日志失败: {ex.Message}"
                });
            }
        }

        private void HandleClearLogs()
        {
            log.Warn("[Log] 清除日志文件");

            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

                if (Directory.Exists(logDir))
                {
                    foreach (var file in Directory.GetFiles(logDir, "*.log"))
                    {
                        File.Delete(file);
                        log.Info($"已删除日志文件: {file}");
                    }
                }

                SendDataToFrontend(new
                {
                    type = "logsCleared",
                    page = PageName,
                    message = "日志已清除",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                log.Error("[Log] 清除日志失败", ex);
                SendDataToFrontend(new
                {
                    type = "error",
                    page = PageName,
                    message = $"清除日志失败: {ex.Message}"
                });
            }
        }
    }
}