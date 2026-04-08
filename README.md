# Middle.net9

<div align="center">

**基于 .NET 9 + React + WebView2 的半导体设备控制系统**

[![.NET Version](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18+-61DAFB?logo=react&logoColor=white)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5+-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

</div>

---

## 📖 项目简介

Middle.net9 是一个现代化的工业控制系统桌面应用程序，采用 **WPF + WebView2 + React** 混合架构，旨在提供流畅的用户界面和强大的设备控制能力。

### ✨ 核心特性

- 🎯 **混合架构**：C# 后端业务逻辑 + React 前端界面
- 🔥 **热重载开发**：开发模式下自动启动 Vite 开发服务器
- ⚡ **高性能通信**：通过 WebView2 实现前后端高效消息传递
- 📝 **配置文件监听**：实时监控 `config.ini` 文件变化并同步到前端
- 🎨 **Material UI**：基于 Google Material Design 的现代化界面
- 🔧 **MVVM 模式**：清晰的代码架构和数据绑定

---

## 🏗️ 技术栈

### 后端 (C#)
- **.NET 9.0** - 最新的 .NET 平台
- **WPF** - Windows Presentation Foundation 桌面框架
- **WebView2** - 嵌入 Chromium 内核浏览器
- **MVVM 模式** - 清晰的业务逻辑分离

### 前端 (React)
- **React 18+** - 声明式 UI 框架
- **TypeScript** - 类型安全的 JavaScript
- **Vite** - 极速的前端构建工具
- **Material UI (MUI)** - React 组件库
- **React Router** - 单页应用路由管理

### 打包与发布
- **Inno Setup** - Windows 安装包制作工具
- **Framework-dependent** - 依赖系统安装的 .NET Runtime

---

## 🚀 快速开始

### 前置要求

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/) (推荐使用 LTS 版本)
- [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) (Evergreen)
- Visual Studio 2022 或 JetBrains Rider

### 安装依赖

#### 1. 克隆项目

#### 2. 安装前端依赖

#### 3. 恢复 .NET 依赖

### 开发模式运行

#### 方式一：使用 Visual Studio（推荐）

1. 用 Visual Studio 2022 打开 `Middle.sln`
2. 按 `F5` 启动调试
3. 应用会自动启动 Vite 开发服务器并加载前端界面

#### 方式二：命令行运行

```bash
# 启动后端服务
dotnet run --project Middle.Backend/Middle.Backend.csproj

# 访问前端界面
npm run dev --prefix Middle.Frontend
```

### 生产模式发布

1. 使用 Visual Studio 发布后端服务
2. 前往 `Middle.Frontend/dist` 目录
3. 使用 Inno Setup 制作安装包

---

### 🔑 常见问题

#### Q：如何更改配置文件路径？

A：修改 `Middle.Backend/appsettings.json` 中的 `ConfigFilePath` 设置。

#### Q：如何添加新的前端依赖？

A：请在 `Middle.Frontend/package.json` 中添加依赖后，运行 `npm install`。

---

### ⌨️ 键盘快捷键

- **F12** - 打开 Chrome 开发者工具（调试前端）
- **Ctrl+R** - 刷新前端页面
- **Ctrl+Shift+I** - 打开开发者工具（备用）

---

## 📁 项目结构

```
Middle.
├── Backend             # 后端代码
│   ├── Controllers     # API 控制器
│   ├── Models          # 数据模型
│   └── Services        # 业务逻辑
├── Frontend            # 前端代码
│   ├── public          # 公共静态文件
│   ├── src             # 源代码
│   |   ├── components  # React 组件
│   |   ├── hooks       # 自定义 Hook
│   |   └── pages       # 页面
│   └── package.json    # 前端依赖配置
├── Middle.sln          # 解决方案文件
└── README.md           # 本文档
```

---

## 🔌 前后端通信机制

### 后端 → 前端发送消息

```csharp
// 中间件：接收来自前端的消息并处理
app.Use(async (context, next) =>
{
    // 只处理 WebSocket 请求
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using (var webSocket = await context.WebSockets.AcceptWebSocketAsync())
            {
                // 处理 WebSocket 消息
                await ReceiveMessages(webSocket);
            }
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});

```

### 前端 → 后端发送消息

```javascript
// 示例：在 React 组件中通过 WebSocket 发送消息
const sendMessage = (message) => {
    // 请确保 websocket 连接已建立
    websocket.send(JSON.stringify(message));
};

```

---

## ⚙️ 配置文件说明

项目使用 `config.ini` 存储运行时配置，格式如下：

```ini
[General]
ConfigFilePath=C:\Path\To\Your\config.ini
LogFilePath=C:\Path\To\Your\log.txt
```

- `ConfigFilePath`：配置文件的绝对路径
- `LogFilePath`：日志文件的绝对路径

> **注意**：如需修改路径，请确保对应文件夹存在且具有读写权限。

**配置项说明：**

- `StationName` - 工作站名称
- `DeviceIp` - 设备 IP 地址
- `DevicePort` - 设备端口号
- `PollingIntervalMs` - 数据轮询间隔（毫秒）
- `AutoStart` - 启动时是否自动连接设备
- `Theme` - 界面主题（light/dark）

配置文件会被实时监听，修改后自动同步到前端界面。

---

## 📦 发布与打包

### 生成 Release 版本

### 使用 Inno Setup 制作安装包

根据项目配置：
- ✅ 使用 **framework-dependent** 发布（需要系统已安装 .NET Runtime）
- ✅ Release 构建时自动执行 Inno Setup
- ⚠️ 不集成 WebView2 Runtime 离线包（减小体积）

用户需要先安装：
1. [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)
2. [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/)

---

## 🛠️ 开发指南

### 添加新的前端页面

1. 在 `my-ui/src/pages/` 创建新组件：

```shell
mkdir -p my-ui/src/pages/NewPage
touch my-ui/src/pages/NewPage/index.tsx
```
2. 在 `App.tsx` 中添加路由：
```tsx
import React from 'react';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';
import NewPage from './pages/NewPage';

function App() {
  return (
    <Router>
      <Switch>
        <Route path="/new" component={NewPage} />
        {/* 其他路由 */}
      </Switch>
    </Router>
  );
}

export default App;
```

### 调试后端服务

- 建议使用 Visual Studio 的调试功能
- 断点调试时注意 WebSocket 连接状态

### 样式与主题

- 样式文件位于 `my-ui/src/styles` 目录
- 主题配置请修改 `config.ini` 中的 `Theme` 项

---

## 📚 学习资源

项目包含详细的学习指南：
- 📖 [ReactUI学习指南.md](ReactUI学习指南.md) - React/TypeScript 完整教程

推荐外部资源：
- [React 官方文档](https://react.dev/)
- [.NET 9 新特性](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-9/overview)
- [WebView2 指南](https://learn.microsoft.com/microsoft-edge/webview2/)
- [Material UI 文档](https://mui.com/)

---

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

---

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

---

## 👨‍💻 作者

**texloer**

- GitHub: [@texloer](https://github.com/texloer)
- Repository: [Middle.net9](https://github.com/texloer/Middle.net9)

---

## 🙏 致谢

- [.NET Foundation](https://dotnetfoundation.org/)
- [React Team](https://react.dev/community/team)
- [Vite](https://vitejs.dev/)
- [Material UI](https://mui.com/)

---

<div align="center">

**如果这个项目对您有帮助，请给个 ⭐ Star 支持一下！**

Made with ❤️ using .NET 9 and React

</div>

### 添加后端业务逻辑

在 `MainViewModel.cs` 的 `HandleFrontendMessage` 方法中处理新的消息类型：
