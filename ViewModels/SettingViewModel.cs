using SemiconductorControlSystem.Models;
using System.IO;
using System.Text.Json;

namespace SemiconductorControlSystem.ViewModels
{
    /// <summary>
    /// 设置页面 ViewModel - 处理配置管理
    /// </summary>
    public class SettingViewModel : BaseViewModel
    {
        private readonly string _configFilePath;
        private FileSystemWatcher? _configWatcher;
        private bool _isSavingConfig;

        public override string PageName => "setting";

        public SettingViewModel()
        {
            _configFilePath = GetConfigFilePath();
            EnsureConfigFileExists();
            StartConfigWatcher();
        }

        public override void HandleMessage(string action, string? data)
        {
            log.Info($"[Setting] 处理消息 - Action: {action}");

            switch (action)
            {
                case "getConfig":
                    HandleGetConfig();
                    break;

                case "updateConfig":
                    HandleUpdateConfig(data);
                    break;

                case "resetConfig":
                    HandleResetConfig();
                    break;

                default:
                    log.Warn($"[Setting] 未识别的操作: {action}");
                    SendDataToFrontend(new
                    {
                        type = "error",
                        page = PageName,
                        message = $"未识别的操作: {action}"
                    });
                    break;
            }
        }

        private void HandleGetConfig()
        {
            log.Debug("[Setting] 加载配置");

            try
            {
                AppConfig config = LoadConfig();
                SendDataToFrontend(new
                {
                    type = "configLoaded",
                    page = PageName,
                    config,
                    configPath = _configFilePath,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                log.Error("[Setting] 加载配置失败", ex);
                SendDataToFrontend(new
                {
                    type = "error",
                    page = PageName,
                    message = $"加载配置失败: {ex.Message}"
                });
            }
        }

        private void HandleUpdateConfig(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                log.Error("[Setting] 更新配置失败：未收到配置数据");
                SendDataToFrontend(new
                {
                    type = "configSaveFailed",
                    page = PageName,
                    message = "未收到配置数据"
                });
                return;
            }

            try
            {
                AppConfig? config = JsonSerializer.Deserialize<AppConfig>(data, _jsonSerializerOptions);
                if (config == null)
                {
                    throw new Exception("配置数据格式无效");
                }

                log.Info($"[Setting] 保存配置: StationName={config.StationName}, DeviceIp={config.DeviceIp}");
                SaveConfig(config);

                SendDataToFrontend(new
                {
                    type = "configSaved",
                    page = PageName,
                    message = "配置已保存",
                    config,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                log.Error("[Setting] 保存配置失败", ex);
                SendDataToFrontend(new
                {
                    type = "configSaveFailed",
                    page = PageName,
                    message = $"保存失败: {ex.Message}"
                });
            }
        }

        private void HandleResetConfig()
        {
            log.Info("[Setting] 重置配置到默认值");

            try
            {
                AppConfig defaultConfig = new();
                SaveConfig(defaultConfig);

                SendDataToFrontend(new
                {
                    type = "configReset",
                    page = PageName,
                    message = "配置已重置为默认值",
                    config = defaultConfig,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                log.Error("[Setting] 重置配置失败", ex);
                SendDataToFrontend(new
                {
                    type = "error",
                    page = PageName,
                    message = $"重置失败: {ex.Message}"
                });
            }
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
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _configWatcher.Changed += OnConfigFileChanged;
            log.Info("配置文件监听器已启动");
        }

        private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            if (_isSavingConfig) return;

            log.Info($"配置文件外部变更: {e.ChangeType}");

            Task.Run(async () =>
            {
                await Task.Delay(200);
                SendDataToFrontend(new
                {
                    type = "configUpdated",
                    page = PageName,
                    message = "config.ini 已更新",
                    config = LoadConfig(),
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            });
        }

        private AppConfig LoadConfig()
        {
            // 复用 MainViewModel 中的逻辑，或提取到公共服务
            // 这里简化处理
            return new AppConfig();
        }

        private void SaveConfig(AppConfig config)
        {
            _isSavingConfig = true;
            try
            {
                WriteConfigToFile(config);
            }
            finally
            {
                _isSavingConfig = false;
            }
        }

        private void WriteConfigToFile(AppConfig config)
        {
            var lines = new List<string>
            {
                "[Config]",
                $"StationName={config.StationName}",
                $"DeviceIp={config.DeviceIp}",
                $"DevicePort={config.DevicePort}",
                $"PollingIntervalMs={config.PollingIntervalMs}",
                $"AutoStart={config.AutoStart.ToString().ToLowerInvariant()}",
                $"Theme={config.Theme}",
                string.Empty
            };

            File.WriteAllLines(_configFilePath, lines);
            log.Debug("配置文件写入成功");
        }

        public override void Dispose()
        {
            if (_configWatcher != null)
            {
                _configWatcher.Changed -= OnConfigFileChanged;
                _configWatcher.Dispose();
            }
            base.Dispose();
        }
    }
}