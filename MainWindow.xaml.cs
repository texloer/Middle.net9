using SemiconductorControlSystem.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using Microsoft.Web.WebView2.Core;

namespace SemiconductorControlSystem
{
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainWindow));

        private MessageRouter _messageRouter;  // 替换原来的 MainViewModel
        private Process? _viteProcess;

        public MainWindow()
        {
            log.Info("主窗口构造函数开始");
            
            InitializeComponent();

            // 初始化消息路由器
            _messageRouter = new MessageRouter();

            log.Debug("消息路由器已初始化");

            // 窗口加载后立刻初始化 WebView
            this.Loaded += MainWindow_Loaded;

            // 添加键盘快捷键支持
            this.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.F12 && MyWebView.CoreWebView2 != null)
                {
                    log.Debug("F12 - 打开开发者工具");
                    MyWebView.CoreWebView2.OpenDevToolsWindow();
                }
                else if (e.Key == System.Windows.Input.Key.R && 
                         System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control &&
                         MyWebView.CoreWebView2 != null)
                {
                    log.Debug("Ctrl+R - 刷新页面");
                    MyWebView.CoreWebView2.Reload();
                }
            };

            log.Info("主窗口构造完成");
        }

        /// <summary>
        /// 从当前运行目录向上查找项目根目录
        /// </summary>
        private string? FindProjectRoot()
        {
            log.Debug("开始查找项目根目录");
            var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (current != null)
            {
                bool hasProjectFile = File.Exists(Path.Combine(current.FullName, "Middle.csproj"));
                bool hasMyUiFolder = Directory.Exists(Path.Combine(current.FullName, "my-ui"));

                if (hasProjectFile || hasMyUiFolder)
                {
                    log.Info($"找到项目根目录: {current.FullName}");
                    return current.FullName;
                }

                current = current.Parent;
            }

            log.Warn("未找到项目根目录");
            return null;
        }

        /// <summary>
        /// 检测端口是否被占用（即服务器是否在运行）
        /// </summary>
        private bool IsPortInUse(int port)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpListeners = ipGlobalProperties.GetActiveTcpListeners();
            bool inUse = tcpListeners.Any(x => x.Port == port);
            
            log.Debug($"端口 {port} 占用检测: {(inUse ? "已占用" : "可用")}");
            return inUse;
        }

        /// <summary>
        /// 自动启动 Vite 开发服务器
        /// </summary>
        private async Task<bool> EnsureDevServerRunningAsync()
        {
            const int vitePort = 5173;
            const int maxRetries = 20; // 最多等待 20 秒

            log.Info("开始检查 Vite 开发服务器状态");

            // 如果服务器已经在运行，直接返回
            if (IsPortInUse(vitePort))
            {
                log.Info($"Vite 开发服务器已在端口 {vitePort} 运行");
                _messageRouter.SendMessageToFrontend?.Invoke("Vite 开发服务器已在运行，正在连接...");
                return true;
            }

            try
            {
                string? projectRoot = FindProjectRoot();
                if (string.IsNullOrWhiteSpace(projectRoot))
                {
                    log.Error("无法定位项目根目录");
                    MessageBox.Show("错误：无法定位项目根目录！", "启动失败");
                    return false;
                }

                string myUiPath = Path.Combine(projectRoot, "my-ui");

                if (!Directory.Exists(myUiPath))
                {
                    log.Error($"未找到 my-ui 目录，路径: {myUiPath}");
                    MessageBox.Show($"错误：未找到 my-ui 目录！\n路径: {myUiPath}", "启动失败");
                    return false;
                }

                log.Info($"准备启动 Vite 服务器，工作目录: {myUiPath}");
                _messageRouter.SendMessageToFrontend?.Invoke("正在启动 Vite 开发服务器...");

                // 启动 npm run dev
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c npm run dev",
                    WorkingDirectory = myUiPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                _viteProcess = Process.Start(startInfo);
                log.Info($"Vite 进程已启动，PID: {_viteProcess?.Id}");

                // 等待服务器启动（最多 20 秒）
                for (int i = 0; i < maxRetries; i++)
                {
                    await Task.Delay(1000);
                    if (IsPortInUse(vitePort))
                    {
                        log.Info($"✅ Vite 开发服务器启动成功！耗时 {i + 1} 秒");
                        _messageRouter.SendMessageToFrontend?.Invoke($"✅ Vite 开发服务器启动成功！({i + 1}秒)");
                        return true;
                    }
                    _messageRouter.SendMessageToFrontend?.Invoke($"等待 Vite 服务器启动... ({i + 1}/{maxRetries}秒)");
                    log.Debug($"等待 Vite 启动... ({i + 1}/{maxRetries})");
                }

                log.Error($"Vite 服务器启动超时（{maxRetries}秒）");
                MessageBox.Show("Vite 开发服务器启动超时！\n请检查 my-ui 目录中的 node_modules 是否存在。", "启动失败");
                return false;
            }
            catch (Exception ex)
            {
                log.Error("启动 Vite 服务器时出错", ex);
                MessageBox.Show($"启动 Vite 服务器时出错：\n{ex.Message}", "错误");
                return false;
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            log.Info("========================================");
            log.Info("主窗口开始加载");
            log.Info($"基础目录: {AppDomain.CurrentDomain.BaseDirectory}");
            
            try
            {
                // 移除不必要的本地文件访问代码，直接使用默认配置
                log.Info("正在初始化 WebView2 核心...");

                await MyWebView.EnsureCoreWebView2Async(null);
                log.Info("WebView2 核心初始化完成");

                // 设置消息发送回调
                _messageRouter.SendMessageToFrontend = (msg) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MyWebView.CoreWebView2.PostWebMessageAsString(msg);
                        log.Debug($"发送消息到前端: {msg}");
                    });
                };

                // 注册前端消息接收器
                if (MyWebView.CoreWebView2 != null)
                {
                    MyWebView.CoreWebView2.WebMessageReceived += OnFrontendMessage;
                    log.Debug("前端消息接收器已注册");
                }

#if DEBUG
                log.Info("运行模式: DEBUG - 连接 Vite 开发服务器");
                bool devServerReady = await EnsureDevServerRunningAsync();
                if (!devServerReady)
                {
                    log.Error("开发服务器未就绪");
                    MessageBox.Show("无法启动开发服务器", "警告");
                    return;
                }
                MyWebView.Source = new Uri("http://localhost:5173");
                log.Info("已导航到 Vite 开发服务器");
#else
                log.Info("运行模式: RELEASE - 加载本地静态文件");
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string localFolder = Path.Combine(baseDir, "WebAsset");
                MyWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "smart.factory",
                    localFolder,
                    CoreWebView2HostResourceAccessKind.Allow
                );
                MyWebView.Source = new Uri($"http://smart.factory/index.html?nocache={DateTime.Now.Ticks}");
#endif

                log.Info("主窗口加载完成");
                log.Info("========================================");
            }
            catch (Exception ex)
            {
                log.Fatal("WebView2 初始化失败", ex);
                MessageBox.Show($"初始化失败：\n{ex.Message}", "错误");
            }
        }

        private void OnFrontendMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            //MessageBox.Show("收到前端消息！"); // 临时测试用
            string message = e.TryGetWebMessageAsString();
            log.Debug($"收到前端消息: {message}");
            _messageRouter.RouteMessage(message);
        }

        /// <summary>
        /// 窗口关闭时清理资源
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            log.Info("主窗口关闭，开始清理资源");
            
            base.OnClosed(e);

            _messageRouter.Dispose();
            log.Debug("消息路由器已释放");

            // 如果是我们启动的 Vite 服务器，关闭时停止它
            if (_viteProcess != null && !_viteProcess.HasExited)
            {
                try
                {
                    log.Info($"正在终止 Vite 进程 (PID: {_viteProcess.Id})");
                    // 终止进程树（包括子进程）
                    _viteProcess.Kill(true);
                    _viteProcess.Dispose();
                    log.Info("Vite 进程已终止");
                }
                catch (Exception ex)
                {
                    log.Error("关闭 Vite 服务器时出错", ex);
                }
            }

            log.Info("资源清理完成");
        }

        // FindProjectRoot, IsPortInUse, EnsureDevServerRunningAsync 方法保持不变
    }
}