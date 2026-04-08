using Microsoft.Web.WebView2.Core;
using SemiconductorControlSystem.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;

namespace SemiconductorControlSystem
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private Process? _viteProcess;

        public MainWindow()
        {
            InitializeComponent();

            // 初始化 ViewModel
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            // 窗口加载后立刻初始化 WebView
            this.Loaded += MainWindow_Loaded;

            // 添加键盘快捷键支持
            this.KeyDown += (s, e) =>
            {
                // F12 - 打开开发者工具
                if (e.Key == System.Windows.Input.Key.F12 && MyWebView.CoreWebView2 != null)
                {
                    MyWebView.CoreWebView2.OpenDevToolsWindow();
                }
                // Ctrl+R - 刷新页面
                else if (e.Key == System.Windows.Input.Key.R && 
                         System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control &&
                         MyWebView.CoreWebView2 != null)
                {
                    MyWebView.CoreWebView2.Reload();
                }
            };
        }

        /// <summary>
        /// 从当前运行目录向上查找项目根目录
        /// </summary>
        private string? FindProjectRoot()
        {
            var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (current != null)
            {
                bool hasProjectFile = File.Exists(Path.Combine(current.FullName, "Middle.csproj"));
                bool hasMyUiFolder = Directory.Exists(Path.Combine(current.FullName, "my-ui"));

                if (hasProjectFile || hasMyUiFolder)
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            return null;
        }

        /// <summary>
        /// 检测端口是否被占用（即服务器是否在运行）
        /// </summary>
        private bool IsPortInUse(int port)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpListeners = ipGlobalProperties.GetActiveTcpListeners();
            return tcpListeners.Any(x => x.Port == port);
        }

        /// <summary>
        /// 自动启动 Vite 开发服务器
        /// </summary>
        private async Task<bool> EnsureDevServerRunningAsync()
        {
            const int vitePort = 5173;
            const int maxRetries = 20; // 最多等待 20 秒

            // 如果服务器已经在运行，直接返回
            if (IsPortInUse(vitePort))
            {
                _viewModel.StatusMessage = "Vite 开发服务器已在运行，正在连接...";
                return true;
            }

            try
            {
                string? projectRoot = FindProjectRoot();
                if (string.IsNullOrWhiteSpace(projectRoot))
                {
                    MessageBox.Show("错误：无法定位项目根目录！", "启动失败");
                    return false;
                }

                string myUiPath = Path.Combine(projectRoot, "my-ui");

                if (!Directory.Exists(myUiPath))
                {
                    MessageBox.Show($"错误：未找到 my-ui 目录！\n路径: {myUiPath}", "启动失败");
                    return false;
                }

                _viewModel.StatusMessage = "正在启动 Vite 开发服务器...";

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

                // 等待服务器启动（最多 20 秒）
                for (int i = 0; i < maxRetries; i++)
                {
                    await Task.Delay(1000);
                    if (IsPortInUse(vitePort))
                    {
                        _viewModel.StatusMessage = $"✅ Vite 开发服务器启动成功！({i + 1}秒)";
                        return true;
                    }
                    _viewModel.StatusMessage = $"等待 Vite 服务器启动... ({i + 1}/{maxRetries}秒)";
                }

                MessageBox.Show("Vite 开发服务器启动超时！\n请检查 my-ui 目录中的 node_modules 是否存在。", "启动失败");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动 Vite 服务器时出错：\n{ex.Message}", "错误");
                return false;
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. 定义路径
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                // 2. 每次启动都删除缓存文件夹，确保加载最新内容
                string userDataFolder = Path.Combine(Path.GetTempPath(), $"Semiconductor_HMI_Cache_{DateTime.Now.Ticks}");

                string runtimeVersion;
                try
                {
                    runtimeVersion = CoreWebView2Environment.GetAvailableBrowserVersionString();
                }
                catch
                {
                    MessageBox.Show(
                        "未检测到系统已安装的 WebView2 Runtime。\n请先安装 Microsoft Edge WebView2 Runtime（Evergreen）。",
                        "启动失败");
                    return;
                }

                // 3. 禁用所有缓存的环境选项
                var options = new CoreWebView2EnvironmentOptions
                {
                    AdditionalBrowserArguments = "--disable-web-security --disable-cache --disable-application-cache --disable-offline-load-stale-cache --disk-cache-size=1 --media-cache-size=1"
                };

                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
                await MyWebView.EnsureCoreWebView2Async(env);

                // 4. 禁用所有缓存设置
                MyWebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;

                _viewModel.SendMessageToFrontend = (msg) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MyWebView.CoreWebView2.PostWebMessageAsString(msg);
                    });
                };

                if (MyWebView.CoreWebView2 != null)
                {
                    MyWebView.CoreWebView2.WebMessageReceived += OnFrontendMessage;
                }

                // 5. 根据编译模式选择加载方式
#if DEBUG
                // 开发模式：自动启动并连接 Vite 开发服务器
                bool devServerReady = await EnsureDevServerRunningAsync();
                if (!devServerReady)
                {
                    MessageBox.Show("无法启动开发服务器，请手动运行 start-dev-server.ps1", "警告");
                    return;
                }

                MyWebView.Source = new Uri("http://localhost:5173");
                _viewModel.StatusMessage = $"开发模式 - 已连接 Vite 开发服务器 (WebView2 Runtime: {runtimeVersion}) - {DateTime.Now:HH:mm:ss}";
#else
                // 发布模式：加载本地静态文件
                string localFolder = Path.Combine(baseDir, "WebAsset");
                if (!Directory.Exists(localFolder))
                {
                    MessageBox.Show($"未找到前端静态资源目录：\n{localFolder}", "启动失败");
                    return;
                }

                MyWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "smart.factory",
                    localFolder,
                    CoreWebView2HostResourceAccessKind.Allow
                );

                string timestamp = DateTime.Now.Ticks.ToString();
                MyWebView.Source = new Uri($"http://smart.factory/index.html?nocache={timestamp}");
                _viewModel.StatusMessage = $"发布模式 - 已加载静态资源 (WebView2 Runtime: {runtimeVersion}) - {DateTime.Now:HH:mm:ss}";
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2 初始化过程中发生异常：\n{ex.Message}", "系统错误");
            }
        }

        private void OnFrontendMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            // 接收来自 React 的消息
            string message = e.TryGetWebMessageAsString();

            // 传递给 ViewModel 处理（ViewModel会自动处理线程安全）
            _viewModel.HandleFrontendMessage(message);
        }

        /// <summary>
        /// 窗口关闭时清理资源
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _viewModel.Dispose();

            // 如果是我们启动的 Vite 服务器，关闭时停止它
            if (_viteProcess != null && !_viteProcess.HasExited)
            {
                try
                {
                    // 终止进程树（包括子进程）
                    _viteProcess.Kill(true);
                    _viteProcess.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"关闭 Vite 服务器时出错: {ex.Message}");
                }
            }
        }
    }
}