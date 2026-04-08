using System.ComponentModel;

namespace SemiconductorControlSystem.ViewModels
{
    /// <summary>
    /// 页面 ViewModel 基础接口
    /// </summary>
    public interface IPageViewModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// 页面名称（对应前端路由）
        /// </summary>
        string PageName { get; }

        /// <summary>
        /// 处理来自前端的消息
        /// </summary>
        /// <param name="action">操作类型</param>
        /// <param name="data">消息数据（JSON字符串）</param>
        void HandleMessage(string action, string? data);

        /// <summary>
        /// 向前端发送消息的回调
        /// </summary>
        Action<string>? SendMessageToFrontend { get; set; }
    }
}