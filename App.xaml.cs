using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using log4net;
using log4net.Config;

namespace Middle
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 初始化 log4net
            InitializeLogger();

            log.Info("========================================");
            log.Info("应用程序启动");
            log.Info($"版本: {Assembly.GetExecutingAssembly().GetName().Version}");
            log.Info($"运行目录: {AppDomain.CurrentDomain.BaseDirectory}");
            log.Info("========================================");

            // 全局异常处理
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void InitializeLogger()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string configPath = Path.Combine(baseDir, "log4net.config");

                // 先显示路径信息以便调试
                string debugInfo = $"基础目录: {baseDir}\n配置文件路径: {configPath}\n文件存在: {File.Exists(configPath)}";

                if (File.Exists(configPath))
                {
                    var configFile = new FileInfo(configPath);
                    XmlConfigurator.ConfigureAndWatch(configFile);

                    // 测试日志是否工作
                    log.Info("log4net 配置加载成功");
                    log.Debug($"配置文件: {configPath}");
                }
                else
                {
                    // 显示错误信息
                    MessageBox.Show(
                        $"未找到 log4net.config 文件！\n\n{debugInfo}",
                        "日志配置错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    // 使用程序集属性配置（备用方案）
                    XmlConfigurator.Configure();
                }

                // 强制刷新日志
                var repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
                log.Info($"Log4net 仓库: {repository.Name}, Appenders: {repository.GetAppenders().Length}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"日志系统初始化失败:\n{ex.Message}\n\n堆栈:\n{ex.StackTrace}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                log.Fatal("未处理的异常 (AppDomain)", ex);
                MessageBox.Show($"应用程序遇到严重错误:\n{ex.Message}", "严重错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            log.Error("未处理的 UI 线程异常", e.Exception);
            MessageBox.Show($"UI 错误:\n{e.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; // 阻止程序崩溃
        }

        protected override void OnExit(ExitEventArgs e)
        {
            log.Info("应用程序退出");
            log.Info("========================================");
            base.OnExit(e);
        }
    }
}
