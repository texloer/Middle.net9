using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using System.Text.Json;

namespace SemiconductorControlSystem.ViewModels
{
    /// <summary>
    /// 消息路由器 - 根据消息内容分发到对应的 ViewModel
    /// </summary>
    public class MessageRouter : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MessageRouter));
        private readonly Dictionary<string, IPageViewModel> _viewModels = new();

        private Action<string>? _sendMessageToFrontend;

        public MessageRouter()
        {
            log.Info("消息路由器初始化");

            // 注册所有 ViewModel
            RegisterViewModel(new HomeViewModel());
            RegisterViewModel(new SettingViewModel());
            RegisterViewModel(new LogViewModel());

            log.Info($"已注册 {_viewModels.Count} 个 ViewModel");
        }

        /// <summary>
        /// 设置向前端发送消息的回调（设置时会同步分发到已注册的所有 ViewModel）
        /// </summary>
        public Action<string>? SendMessageToFrontend
        {
            get => _sendMessageToFrontend;
            set
            {
                _sendMessageToFrontend = value;
                // 将回调分发给所有已注册的 ViewModel，保证它们能正常发送消息到前端
                foreach (var vm in _viewModels.Values)
                {
                    vm.SendMessageToFrontend = _sendMessageToFrontend;
                }
            }
        }

        /// <summary>
        /// 注册 ViewModel
        /// </summary>
        private void RegisterViewModel(IPageViewModel viewModel)
        {
            _viewModels[viewModel.PageName] = viewModel;
            // 当注册时，如果路由器已经有回调，立即注入到新注册的 ViewModel
            viewModel.SendMessageToFrontend = _sendMessageToFrontend;
            log.Debug($"注册 ViewModel: {viewModel.PageName} ({viewModel.GetType().Name})");
        }

        /// <summary>
        /// 路由前端消息到对应的 ViewModel
        /// </summary>
        public void RouteMessage(string message)
        {
            log.Debug($"收到前端消息: {message}");

            try
            {
                using var document = JsonDocument.Parse(message);
                var root = document.RootElement;

                // 提取 page 和 action
                if (!root.TryGetProperty("page", out var pageElement))
                {
                    log.Warn("消息缺少 'page' 字段，尝试使用默认页面");
                    HandleLegacyMessage(message);
                    return;
                }

                string page = pageElement.GetString() ?? "";
                string action = root.TryGetProperty("action", out var actionElement)
                    ? actionElement.GetString() ?? ""
                    : "";

                string? data = root.TryGetProperty("data", out var dataElement)
                    ? dataElement.GetRawText()
                    : null;

                log.Info($"路由消息 -> Page: {page}, Action: {action}");

                // 查找对应的 ViewModel
                if (_viewModels.TryGetValue(page, out var viewModel))
                {
                    viewModel.HandleMessage(action, data);
                }
                else
                {
                    log.Warn($"未找到页面 '{page}' 对应的 ViewModel");
                    SendErrorToFrontend(page, $"未找到页面处理器: {page}");
                }
            }
            catch (JsonException ex)
            {
                log.Error("JSON 解析失败", ex);
                HandleLegacyMessage(message);
            }
            catch (Exception ex)
            {
                log.Error("消息路由失败", ex);
            }
        }

        /// <summary>
        /// 处理旧格式消息（兼容性）
        /// </summary>
        private void HandleLegacyMessage(string message)
        {
            log.Debug("尝试使用传统方式处理消息");

            try
            {
                using var document = JsonDocument.Parse(message);
                var root = document.RootElement;

                if (root.TryGetProperty("action", out var actionElement))
                {
                    string action = actionElement.GetString() ?? "";

                    // 默认路由到 home 页面
                    if (_viewModels.TryGetValue("home", out var homeVm))
                    {
                        log.Info($"使用默认路由到 home 页面，action: {action}");
                        homeVm.HandleMessage(action, null);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("传统消息处理失败", ex);
            }
        }

        /// <summary>
        /// 向前端发送错误消息
        /// </summary>
        private void SendErrorToFrontend(string page, string message)
        {
            var errorMessage = JsonSerializer.Serialize(new
            {
                type = "error",
                page,
                message,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });

            // 优先通过路由器的统一回调发送；若未设置回调则尝试使用任意一个 ViewModel 的回调
            if (_sendMessageToFrontend != null)
            {
                _sendMessageToFrontend.Invoke(errorMessage);
                return;
            }

            _viewModels.Values.FirstOrDefault()?.SendMessageToFrontend?.Invoke(errorMessage);
        }

        public void Dispose()
        {
            log.Info("消息路由器释放资源");

            foreach (var vm in _viewModels.Values)
            {
                vm.Dispose();
            }

            _viewModels.Clear();
        }
    }
}