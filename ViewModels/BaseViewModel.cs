using log4net;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Threading;

namespace SemiconductorControlSystem.ViewModels
{
    /// <summary>
    /// ViewModel 基类，提供通用功能
    /// </summary>
    public abstract class BaseViewModel : IPageViewModel
    {
        protected readonly ILog log;
        protected readonly Dispatcher _uiDispatcher;
        protected readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public abstract string PageName { get; }
        public Action<string>? SendMessageToFrontend { get; set; }

        protected BaseViewModel()
        {
            log = LogManager.GetLogger(GetType());
            _uiDispatcher = Dispatcher.CurrentDispatcher;
            log.Info($"{PageName} ViewModel 初始化");
        }

        public abstract void HandleMessage(string action, string? data);

        /// <summary>
        /// 向前端发送数据
        /// </summary>
        protected void SendDataToFrontend(object data)
        {
            try
            {
                if (SendMessageToFrontend == null)
                {
                    log.Warn($"[{PageName}] SendMessageToFrontend 委托未注册，无法发送消息到前端");
                    return;
                }

                string json = JsonSerializer.Serialize(data, _jsonSerializerOptions);
                log.Debug($"[{PageName}] 发送消息到前端: {json}");

                if (_uiDispatcher.CheckAccess())
                {
                    SendMessageToFrontend.Invoke(json);
                }
                else
                {
                    _uiDispatcher.Invoke(() => SendMessageToFrontend.Invoke(json));
                }
            }
            catch (Exception ex)
            {
                log.Error($"[{PageName}] 发送消息到前端失败", ex);
            }
        }

        /// <summary>
        /// 线程安全的属性更新
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;

            if (_uiDispatcher.CheckAccess())
            {
                OnPropertyChanged(propertyName);
            }
            else
            {
                _uiDispatcher.Invoke(() => OnPropertyChanged(propertyName));
            }

            return true;
        }

        private string _status = "";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void Dispose()
        {
            log.Info($"{PageName} ViewModel 释放资源");
        }
    }
}