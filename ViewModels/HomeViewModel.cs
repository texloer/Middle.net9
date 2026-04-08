using log4net;
using System.Text.Json;

namespace SemiconductorControlSystem.ViewModels
{
    /// <summary>
    /// 主页 ViewModel - 处理设备控制相关逻辑
    /// </summary>
    public class HomeViewModel : BaseViewModel
    {
        private string _deviceStatus = "未连接";
        private double _temperature = 0.0;
        private bool _isRunning = false;

        public override string PageName => "home";

        public string DeviceStatus
        {
            get => _deviceStatus;
            set => SetProperty(ref _deviceStatus, value);
        }

        public double Temperature
        {
            get => _temperature;
            set => SetProperty(ref _temperature, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public override void HandleMessage(string action, string? data)
        {
            log.Info($"[Home] 处理消息 - Action: {action}");

            switch (action)
            {
                case "start":
                    HandleStartDevice(data);
                    break;

                case "stop":
                    HandleStopDevice(data);
                    break;

                case "query":
                    HandleQueryStatus(data);
                    break;

                default:
                    log.Warn($"[Home] 未识别的操作: {action}");
                    SendDataToFrontend(new
                    {
                        type = "error",
                        page = PageName,
                        message = $"未识别的操作: {action}",
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                    break;
            }
        }

        private void HandleStartDevice(string? data)
        {
            log.Info("[Home] 启动设备");

            Task.Run(async () =>
            {
                try
                {
                    DeviceStatus = "启动中...";
                    SendDataToFrontend(new
                    {
                        type = "deviceStatus",
                        page = PageName,
                        status = "starting",
                        message = "设备启动中...",
                        timestamp = DateTime.Now.ToString("HH:mm:ss")
                    });

                    // 模拟设备启动延迟
                    await Task.Delay(2000);

                    IsRunning = true;
                    DeviceStatus = "运行中";
                    Temperature = 25.5;

                    log.Info("[Home] 设备启动成功");
                    SendDataToFrontend(new
                    {
                        type = "deviceStatus",
                        page = PageName,
                        status = "running",
                        success = true,
                        message = "设备启动完成",
                        temperature = Temperature,
                        timestamp = DateTime.Now.ToString("HH:mm:ss")
                    });
                }
                catch (Exception ex)
                {
                    log.Error("[Home] 设备启动失败", ex);
                    DeviceStatus = "启动失败";
                    SendDataToFrontend(new
                    {
                        type = "error",
                        page = PageName,
                        success = false,
                        message = $"启动失败: {ex.Message}",
                        timestamp = DateTime.Now.ToString("HH:mm:ss")
                    });
                }
            });
        }

        private void HandleStopDevice(string? data)
        {
            log.Info("[Home] 停止设备");

            Task.Run(async () =>
            {
                try
                {
                    DeviceStatus = "停止中...";
                    SendDataToFrontend(new
                    {
                        type = "deviceStatus",
                        page = PageName,
                        status = "stopping",
                        message = "设备停止中...",
                        timestamp = DateTime.Now.ToString("HH:mm:ss")
                    });

                    await Task.Delay(1500);

                    IsRunning = false;
                    DeviceStatus = "已停止";
                    Temperature = 0.0;

                    log.Info("[Home] 设备停止成功");
                    SendDataToFrontend(new
                    {
                        type = "deviceStatus",
                        page = PageName,
                        status = "stopped",
                        success = true,
                        message = "设备已停止",
                        timestamp = DateTime.Now.ToString("HH:mm:ss")
                    });
                }
                catch (Exception ex)
                {
                    log.Error("[Home] 设备停止失败", ex);
                    SendDataToFrontend(new
                    {
                        type = "error",
                        page = PageName,
                        success = false,
                        message = $"停止失败: {ex.Message}",
                        timestamp = DateTime.Now.ToString("HH:mm:ss")
                    });
                }
            });
        }

        private void HandleQueryStatus(string? data)
        {
            log.Debug("[Home] 查询设备状态");

            SendDataToFrontend(new
            {
                type = "deviceStatus",
                page = PageName,
                status = IsRunning ? "running" : "stopped",
                isRunning = IsRunning,
                temperature = Temperature,
                deviceStatus = DeviceStatus,
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });
        }
    }
}