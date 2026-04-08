using SemiconductorControlSystem.Models;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Threading;
using log4net;

namespace SemiconductorControlSystem.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainViewModel));

        private string _statusMessage = "系统就绪";
        private string _lastReceivedMessage = "";
        private readonly Dispatcher _uiDispatcher;
        private readonly string _configFilePath;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        private FileSystemWatcher? _configWatcher;
        private bool _isSavingConfig;

        /// <summary>
        /// 构造函数，捕获UI线程的Dispatcher
        /// </summary>
        public MainViewModel()
        {
            log.Info("MainViewModel 初始化开始");
            
            _uiDispatcher = Dispatcher.CurrentDispatcher;
            _configFilePath = GetConfigFilePath();

            log.Debug($"配置文件路径: {_configFilePath}");

            EnsureConfigFileExists();
            StartConfigWatcher();

            log.Info("MainViewModel 初始化完成");
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                // 线程安全的属性更新
                if (_uiDispatcher.CheckAccess())
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
                else
                {
                    _uiDispatcher.Invoke(() =>
                    {
                        _statusMessage = value;
                        OnPropertyChanged();
                    });
                }
            }
        }

        public string LastReceivedMessage
        {
            get => _lastReceivedMessage;
            set
            {
                // 线程安全的属性更新
                if (_uiDispatcher.CheckAccess())
                {
                    _lastReceivedMessage = value;
                    OnPropertyChanged();
                }
                else
                {
                    _uiDispatcher.Invoke(() =>
                    {
                        _lastReceivedMessage = value;
                        OnPropertyChanged();
                    });
                }
            }
        }

        /// <summary>
        /// 处理来自前端的消息
        /// </summary>
        public void HandleFrontendMessage(string message)
        {
            log.Debug($"收到前端消息: {message}");
            
            LastReceivedMessage = message;

            if (TryHandleJsonMessage(message))
            {
                return;
            }

            // 这里可以解析JSON并执行具体的业务逻辑
            if (message.Contains("\"action\":\"start\""))
            {
                log.Info("设备启动命令");
                StatusMessage = "设备启动中...";
                SimulateBackgroundTask("设备启动完成");
            }
            else if (message.Contains("\"action\":\"stop\""))
            {
                log.Info("设备停止命令");
                StatusMessage = "设备停止中...";
                SimulateBackgroundTask("设备已停止");
            }
            else if (message.Contains("\"action\":\"query\""))
            {
                log.Info("查询设备状态");
                StatusMessage = "查询设备状态...";
                SimulateBackgroundTask("状态：运行中 | 温度：25.5°C");
            }
            else
            {
                log.Warn($"未识别的消息类型: {message}");
                StatusMessage = $"收到前端消息: {message}";
            }
        }

        private bool TryHandleJsonMessage(string message)
        {
            try
            {
                using var document = JsonDocument.Parse(message);

                if (!document.RootElement.TryGetProperty("action", out var actionElement))
                {
                    return false;
                }

                string? action = actionElement.GetString();
                log.Info($"处理 JSON 消息，action: {action}");

                switch (action)
                {
                    case "start":
                        StatusMessage = "设备启动中...";
                        SimulateBackgroundTask("设备启动完成");
                        return true;

                    case "stop":
                        StatusMessage = "设备停止中...";
                        SimulateBackgroundTask("设备已停止");
                        return true;

                    case "query":
                        StatusMessage = "查询设备状态...";
                        SimulateBackgroundTask("状态：运行中 | 温度：25.5°C");
                        return true;

                    case "getConfig":
                        log.Debug("请求加载配置");
                        StatusMessage = "已加载配置文件";
                        SendConfigToFrontend("configLoaded", "配置已读取");
                        return true;

                    case "updateConfig":
                        if (!document.RootElement.TryGetProperty("data", out var dataElement))
                        {
                            log.Error("更新配置失败：未收到配置数据");
                            SendDataToFrontend(new
                            {
                                type = "configSaveFailed",
                                message = "未收到配置数据",
                                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            });
                            return true;
                        }

                        AppConfig? config = JsonSerializer.Deserialize<AppConfig>(dataElement.GetRawText(), _jsonSerializerOptions);
                        if (config == null)
                        {
                            log.Error("更新配置失败：配置数据格式无效");
                            SendDataToFrontend(new
                            {
                                type = "configSaveFailed",
                                message = "配置数据格式无效",
                                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            });
                            return true;
                        }

                        SaveConfig(config);
                        return true;
                }

                return false;
            }
            catch (JsonException ex)
            {
                log.Error("JSON 解析失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 模拟后台任务（如PLC通信、数据采集等）
        /// </summary>
        private void SimulateBackgroundTask(string resultMessage)
        {
            Task.Run(async () =>
            {
                try
                {
                    log.Debug($"开始后台任务: {resultMessage}");
                    
                    // 模拟耗时操作（如设备通信）
                    await Task.Delay(2000);

                    log.Debug($"后台任务完成: {resultMessage}");

                    // 后台线程更新状态
                    StatusMessage = resultMessage;

                    // 同时向前端发送结果
                    SendDataToFrontend(new 
                    { 
                        success = true, 
                        message = resultMessage,
                        timestamp = DateTime.Now.ToString("HH:mm:ss")
                    });
                }
                catch (Exception ex)
                {
                    log.Error("后台任务执行失败", ex);
                    StatusMessage = $"任务失败: {ex.Message}";
                }
            });
        }

        /// <summary>
        /// 向前端发送消息的回调（由View层设置）
        /// </summary>
        public Action<string>? SendMessageToFrontend { get; set; }

        /// <summary>
        /// 向前端发送数据
        /// </summary>
        public void SendDataToFrontend(object data)
        {
            string json = JsonSerializer.Serialize(data, _jsonSerializerOptions);
            log.Debug($"发送数据到前端: {json}");
            SendMessageToFrontend?.Invoke(json);
        }

        private string GetConfigFilePath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

#if DEBUG
            return Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "config.ini"));
#else
            return Path.Combine(baseDirectory, "config.ini");
#endif
        }

        private void EnsureConfigFileExists()
        {
            string? directory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                log.Info($"创建配置文件目录: {directory}");
            }

            if (!File.Exists(_configFilePath))
            {
                log.Info($"配置文件不存在，创建默认配置: {_configFilePath}");
                WriteConfigToFile(new AppConfig());
            }
        }

        private void StartConfigWatcher()
        {
            string? directory = Path.GetDirectoryName(_configFilePath);
            string? fileName = Path.GetFileName(_configFilePath);

            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(fileName))
            {
                log.Warn("无法启动配置文件监听：路径无效");
                return;
            }

            _configWatcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            _configWatcher.Changed += OnConfigFileChanged;
            _configWatcher.Created += OnConfigFileChanged;
            _configWatcher.Renamed += OnConfigFileChanged;

            log.Info("配置文件监听器已启动");
        }

        private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            if (_isSavingConfig)
            {
                return;
            }

            log.Info($"配置文件变更: {e.ChangeType}");

            _ = Task.Run(async () =>
            {
                await Task.Delay(150);
                SendConfigToFrontend("configUpdated", "config.ini 已更新");
            });
        }

        private void SaveConfig(AppConfig config)
        {
            log.Info($"保存配置: StationName={config.StationName}, DeviceIp={config.DeviceIp}");
            WriteConfigToFile(config);
            StatusMessage = $"配置已保存到 config.ini - {DateTime.Now:HH:mm:ss}";
            SendConfigToFrontend("configSaved", "配置已保存");
        }

        private AppConfig LoadConfig()
        {
            log.Debug("加载配置文件");
            
            AppConfig config = new();
            if (!File.Exists(_configFilePath))
            {
                log.Warn("配置文件不存在");
                return config;
            }

            Dictionary<string, Dictionary<string, string>> sections = ParseIniFile();
            if (!sections.TryGetValue("Config", out var configSection))
            {
                log.Warn("配置文件中未找到 [Config] 节");
                return config;
            }

            if (configSection.TryGetValue("StationName", out var stationName) && !string.IsNullOrWhiteSpace(stationName))
            {
                config.StationName = stationName;
            }

            if (configSection.TryGetValue("DeviceIp", out var deviceIp) && !string.IsNullOrWhiteSpace(deviceIp))
            {
                config.DeviceIp = deviceIp;
            }

            if (configSection.TryGetValue("DevicePort", out var devicePort) && !string.IsNullOrWhiteSpace(devicePort))
            {
                config.DevicePort = devicePort;
            }

            if (configSection.TryGetValue("PollingIntervalMs", out var pollingIntervalMs) && !string.IsNullOrWhiteSpace(pollingIntervalMs))
            {
                config.PollingIntervalMs = pollingIntervalMs;
            }

            if (configSection.TryGetValue("AutoStart", out var autoStart) && bool.TryParse(autoStart, out var autoStartValue))
            {
                config.AutoStart = autoStartValue;
            }

            if (configSection.TryGetValue("Theme", out var theme) && !string.IsNullOrWhiteSpace(theme))
            {
                config.Theme = theme;
            }

            log.Info("配置加载成功");
            return config;
        }

        private Dictionary<string, Dictionary<string, string>> ParseIniFile()
        {
            Dictionary<string, Dictionary<string, string>> sections = new(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(_configFilePath))
            {
                return sections;
            }

            string currentSection = "Config";

            foreach (string rawLine in File.ReadAllLines(_configFilePath))
            {
                string line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';') || line.StartsWith('#'))
                {
                    continue;
                }

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    currentSection = line[1..^1].Trim();
                    if (!sections.ContainsKey(currentSection))
                    {
                        sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }

                    continue;
                }

                int separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                if (!sections.ContainsKey(currentSection))
                {
                    sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                string key = line[..separatorIndex].Trim();
                string value = line[(separatorIndex + 1)..].Trim();
                sections[currentSection][key] = value;
            }

            return sections;
        }

        private void WriteConfigToFile(AppConfig config)
        {
            Dictionary<string, Dictionary<string, string>> sections = ParseIniFile();
            sections["Config"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["StationName"] = config.StationName,
                ["DeviceIp"] = config.DeviceIp,
                ["DevicePort"] = config.DevicePort,
                ["PollingIntervalMs"] = config.PollingIntervalMs,
                ["AutoStart"] = config.AutoStart.ToString().ToLowerInvariant(),
                ["Theme"] = config.Theme
            };

            List<string> lines = [];
            foreach (var section in sections)
            {
                lines.Add($"[{section.Key}]");

                foreach (var item in section.Value)
                {
                    lines.Add($"{item.Key}={item.Value}");
                }

                lines.Add(string.Empty);
            }

            _isSavingConfig = true;
            try
            {
                File.WriteAllLines(_configFilePath, lines);
                log.Debug("配置文件写入成功");
            }
            catch (Exception ex)
            {
                log.Error("配置文件写入失败", ex);
                throw;
            }
            finally
            {
                _isSavingConfig = false;
            }
        }

        private void SendConfigToFrontend(string messageType, string statusMessage)
        {
            AppConfig config = LoadConfig();

            SendDataToFrontend(new
            {
                type = messageType,
                message = statusMessage,
                config,
                configPath = _configFilePath,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            log.Info("MainViewModel 释放资源");
            
            if (_configWatcher != null)
            {
                _configWatcher.Changed -= OnConfigFileChanged;
                _configWatcher.Created -= OnConfigFileChanged;
                _configWatcher.Renamed -= OnConfigFileChanged;
                _configWatcher.Dispose();
            }
        }
    }
}
