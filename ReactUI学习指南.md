# React + TypeScript 实战教学指南

> 基于您的 WPF + React WebView2 项目的完整教程

---

## 📚 目录
1. [React 基础概念](#1-react-基础概念)
2. [TypeScript 类型系统](#2-typescript-类型系统)
3. [React Hooks 详解](#3-react-hooks-详解)
4. [组件拆解分析](#4-组件拆解分析)
5. [Material UI 使用](#5-material-ui-使用)
6. [WebView2 通信机制](#6-webview2-通信机制)
7. [React Router 路由](#7-react-router-路由)
8. [实战练习](#8-实战练习)

---

## 1. React 基础概念

### 1.1 什么是组件？
组件是 React 的核心概念，可以理解为"可复用的 UI 模块"。

```typescript
// 函数组件（推荐）- 就像一个返回 HTML 的函数
const Home = () => {
  return <h1>欢迎使用</h1>
}

// 等价于 C# 中的方法
// public string Home() {
//   return "<h1>欢迎使用</h1>";
// }
```

**在您的项目中：**
- `Home.tsx` → 主页组件（设备控制面板）
- `Navigation.tsx` → 导航栏组件
- `Log.tsx` → 日志页面组件

每个 `.tsx` 文件通常包含一个组件，最后用 `export default` 导出。

---

### 1.2 JSX 语法 - "JavaScript 中写 HTML"

```typescript
// JSX 允许在 JavaScript 中写 HTML 标签
const element = <div>Hello World</div>

// 可以嵌入 JavaScript 表达式（用花括号）
const name = "用户"
const greeting = <h1>你好，{name}!</h1>

// 等价于调用 JavaScript 函数
// React.createElement('h1', null, '你好，', name, '!')
```

**在 Home.tsx 中的例子：**
```typescript
<Typography variant="body2" fontWeight={500}>
  <strong>最新消息:</strong> {latestMessage}
</Typography>
// {latestMessage} 是 JavaScript 变量，会动态显示内容
```

---

### 1.3 Props - 组件的"参数"

```typescript
// 定义一个接收参数的组件
interface GreetingProps {
  name: string
  age: number
}

const Greeting = (props: GreetingProps) => {
  return <h1>你好，{props.name}，你 {props.age} 岁了</h1>
}

// 使用组件时传递参数
<Greeting name="张三" age={25} />
```

**类比 C# 方法：**
```csharp
// C# 方法参数
public string Greeting(string name, int age) {
  return $"你好，{name}，你 {age} 岁了";
}
```

---

## 2. TypeScript 类型系统

### 2.1 为什么使用 TypeScript？
TypeScript 是 "带类型的 JavaScript"，类似于 C# 的强类型系统。

```typescript
// ❌ JavaScript - 没有类型检查，容易出错
let age = 25
age = "二十五"  // 不会报错，但可能导致 bug

// ✅ TypeScript - 有类型保护
let age: number = 25
age = "二十五"  // 编译错误：不能将字符串赋值给数字
```

---

### 2.2 接口（Interface）- 定义数据结构

**在 Home.tsx 中定义的接口：**
```typescript
interface BackendMessage {
  success?: boolean;      // ? 表示可选属性
  message?: string;
  timestamp?: string;
}
```

**等价于 C# 类定义：**
```csharp
public class BackendMessage {
  public bool? Success { get; set; }     // nullable
  public string? Message { get; set; }
  public string? Timestamp { get; set; }
}
```

**实际使用：**
```typescript
// 定义一个变量，必须符合接口结构
const msg: BackendMessage = {
  success: true,
  message: "操作成功",
  timestamp: "2024-01-01 10:00:00"
}

// ❌ 错误：success 必须是 boolean，不能是字符串
const badMsg: BackendMessage = {
  success: "true"  // 编译错误
}
```

---

### 2.3 常用类型

```typescript
// 基础类型
let name: string = "张三"
let age: number = 25
let isActive: boolean = true
let nothing: null = null
let notDefined: undefined = undefined

// 数组
let numbers: number[] = [1, 2, 3]
let names: Array<string> = ["张三", "李四"]

// 对象
let user: { name: string; age: number } = {
  name: "张三",
  age: 25
}

// 函数
const add = (a: number, b: number): number => {
  return a + b
}

// 联合类型（可以是多种类型之一）
let id: number | string = 123
id = "ABC"  // ✅ 正确
```

---

## 3. React Hooks 详解

Hooks 是 React 的"魔法函数"，让你在函数组件中使用状态和副作用。

### 3.1 useState - 状态管理

**概念：** 让组件"记住"数据，数据变化时自动刷新界面。

```typescript
import { useState } from 'react'

const Counter = () => {
  // 声明一个状态变量 count，初始值为 0
  // setCount 是更新 count 的函数
  const [count, setCount] = useState<number>(0)
  
  return (
    <div>
      <p>当前计数：{count}</p>
      <button onClick={() => setCount(count + 1)}>点击 +1</button>
    </div>
  )
}
```

**解析：**
```typescript
const [状态变量, 更新函数] = useState<类型>(初始值)
//     ↓          ↓                    ↓
//   count   setCount              0
```

**类比 C# MVVM：**
```csharp
// C# WPF 的 INotifyPropertyChanged
private int _count = 0;
public int Count {
  get => _count;
  set {
    _count = value;
    OnPropertyChanged(nameof(Count));  // 通知界面更新
  }
}

// React 的 useState 自动实现了属性通知
```

---

### 3.2 Home.tsx 中的 useState 使用

```typescript
const [messages, setMessages] = useState<BackendMessage[]>([])
const [latestMessage, setLatestMessage] = useState<string>('')
```

**含义：**
1. `messages` - 存储消息列表（数组）
2. `setMessages` - 更新消息列表的函数
3. `<BackendMessage[]>` - 类型标注：BackendMessage 对象的数组
4. `[]` - 初始值：空数组

**更新状态的例子：**
```typescript
// ❌ 错误：不能直接修改状态
messages.push(newMessage)  // 不会触发界面更新

// ✅ 正确：使用 set 函数
setMessages([...messages, newMessage])  // 创建新数组，触发更新

// ✅ 推荐：使用函数式更新（接收旧值，返回新值）
setMessages(prev => [...prev, newMessage])
```

**`...` 扩展运算符解释：**
```typescript
const oldArray = [1, 2, 3]
const newArray = [...oldArray, 4]  // [1, 2, 3, 4]

// 等价于
const newArray = oldArray.concat(4)

// 或者
const newArray = oldArray.slice()
newArray.push(4)
```

---

### 3.3 useEffect - 副作用处理

**概念：** 在特定时机执行代码（组件加载后、数据变化后等）。

```typescript
import { useEffect } from 'react'

useEffect(() => {
  // 这里的代码会在组件加载后执行
  console.log('组件已加载')
  
  // 可选：返回清理函数（组件卸载时执行）
  return () => {
    console.log('组件即将卸载')
  }
}, [])  // 依赖数组：空数组表示只在加载时执行一次
```

**依赖数组的作用：**
```typescript
// 1. 空数组 [] - 只在组件挂载时执行一次（类似 Loaded 事件）
useEffect(() => {
  console.log('组件加载了')
}, [])

// 2. 有依赖 [count] - count 变化时执行
useEffect(() => {
  console.log('count 变化了:', count)
}, [count])

// 3. 无依赖数组 - 每次渲染都执行（很少用）
useEffect(() => {
  console.log('每次渲染都执行')
})
```

---

### 3.4 Home.tsx 中的 useEffect 使用

```typescript
useEffect(() => {
  // 检查是否在 WebView2 环境中
  if (typeof window.chrome !== 'undefined' && window.chrome.webview) {
    // 注册事件监听器
    window.chrome.webview.addEventListener('message', (event: any) => {
      try {
        const data: BackendMessage = JSON.parse(event.data);
        setMessages(prev => [...prev, data].slice(-10));  // 保留最新 10 条
        setLatestMessage(data.message || '收到未知消息');
      } catch (error) {
        console.error('解析消息失败:', error);
        setLatestMessage(`原始消息: ${event.data}`);
      }
    });
  }
}, []);  // 空数组：只在组件加载时注册一次监听器
```

**逐行解释：**

1. **检查 WebView2 环境：**
   ```typescript
   if (typeof window.chrome !== 'undefined' && window.chrome.webview)
   ```
   - 确保代码只在 WebView2 中运行
   - 在普通浏览器中 `window.chrome.webview` 不存在

2. **注册消息监听器：**
   ```typescript
   window.chrome.webview.addEventListener('message', (event: any) => {
   ```
   - 类似于 C# 的 `myWebView.WebMessageReceived += Handler`
   - 当 C# 发送消息时，这个函数会被调用

3. **解析 JSON 数据：**
   ```typescript
   const data: BackendMessage = JSON.parse(event.data);
   ```
   - C# 发送的是 JSON 字符串，需要解析成对象
   - 类比：`JsonConvert.DeserializeObject<BackendMessage>(jsonString)`

4. **更新消息列表（只保留最新 10 条）：**
   ```typescript
   setMessages(prev => [...prev, data].slice(-10));
   ```
   - `[...prev, data]` - 把新消息追加到列表末尾
   - `.slice(-10)` - 只保留最后 10 个元素

5. **错误处理：**
   ```typescript
   catch (error) {
     setLatestMessage(`原始消息: ${event.data}`);
   }
   ```
   - 如果 JSON 解析失败，直接显示原始数据

---

## 4. 组件拆解分析

让我们逐块分析 `Home.tsx` 组件的结构。

### 4.1 组件整体结构

```typescript
const Home = () => {
  // 1. 状态定义区
  const [messages, setMessages] = useState<BackendMessage[]>([])
  
  // 2. 副作用区（事件监听、数据获取等）
  useEffect(() => { /* ... */ }, [])
  
  // 3. 事件处理函数区
  const sendToBackend = (action: string) => { /* ... */ }
  
  // 4. 渲染区（返回 JSX）
  return (
    <Container>
      {/* UI 元素 */}
    </Container>
  )
}
```

---

### 4.2 发送消息到 C# 后端

```typescript
const sendToBackend = (action: string) => {
  // 检查 WebView2 环境
  if (typeof window.chrome !== 'undefined' && window.chrome.webview) {
    const message = JSON.stringify({ action });  // 转为 JSON 字符串
    window.chrome.webview.postMessage(message);  // 发送到 C#
  } else {
    alert('不在 WebView2 环境中，无法发送消息');
  }
};
```

**对应的 C# 代码：**
```csharp
// MainWindow.xaml.cs
MyWebView.WebMessageReceived += (sender, args) => {
  string jsonMessage = args.WebMessageAsJson;  // 接收 JSON
  var action = JsonSerializer.Deserialize<dynamic>(jsonMessage);
  // 处理消息...
};
```

---

### 4.3 条件渲染 - 显示/隐藏元素

```typescript
{latestMessage && (
  <Alert severity="info">
    最新消息: {latestMessage}
  </Alert>
)}
```

**含义：**
- `latestMessage &&` - 如果 `latestMessage` 有值（非空字符串），则显示后面的内容
- 等价于：`if (latestMessage) { 显示 Alert }`

**更多条件渲染方式：**
```typescript
// 1. && 运算符（推荐用于简单显示/隐藏）
{isLoading && <div>加载中...</div>}

// 2. 三元运算符（二选一）
{isLoading ? <div>加载中...</div> : <div>已加载</div>}

// 3. 传统 if-else（需要提前定义变量）
let content;
if (isLoading) {
  content = <div>加载中...</div>
} else {
  content = <div>已加载</div>
}
return <div>{content}</div>
```

---

### 4.4 列表渲染 - map 函数

```typescript
{messages.map((msg, index) => (
  <ListItem key={index}>
    <ListItemText primary={msg.message} />
  </ListItem>
))}
```

**解释：**
- `.map()` 遍历数组，为每个元素生成一个 JSX 元素
- `key={index}` 是 React 的要求，帮助优化渲染性能
- **类比 C# LINQ：** `messages.Select(msg => new ListItem { ... })`

**详细例子：**
```typescript
const numbers = [1, 2, 3]

// map 转换数组
const doubled = numbers.map(num => num * 2)  // [2, 4, 6]

// map 生成 JSX
const listItems = numbers.map(num => <li key={num}>{num}</li>)
// 结果：[<li>1</li>, <li>2</li>, <li>3</li>]
```

---

### 4.5 事件处理

```typescript
<Button onClick={() => sendToBackend('start')}>
  启动设备
</Button>
```

**箭头函数语法：**
```typescript
// 传统函数
function add(a, b) {
  return a + b
}

// 箭头函数（简洁写法）
const add = (a, b) => a + b

// 多行箭头函数
const add = (a, b) => {
  const result = a + b
  return result
}

// 无参数箭头函数
const sayHi = () => console.log('Hi')
```

**为什么用箭头函数？**
```typescript
// ❌ 错误：会立即执行
<Button onClick={sendToBackend('start')}>

// ✅ 正确：传递函数引用，点击时才执行
<Button onClick={() => sendToBackend('start')}>
```

---

## 5. Material UI 使用

Material UI 是一个 React 组件库，提供预制的漂亮组件。

### 5.1 基础组件使用

```typescript
import { Button, TextField, Card } from '@mui/material'

const MyComponent = () => {
  return (
    <Card>
      <TextField label="用户名" />
      <Button variant="contained">提交</Button>
    </Card>
  )
}
```

---

### 5.2 sx 属性 - 内联样式

```typescript
<Box sx={{ 
  py: 4,                    // padding-top 和 padding-bottom: 4 * 8px = 32px
  textAlign: 'center',      // 文本居中
  color: 'primary.main',    // 使用主题颜色
  '&:hover': {              // 悬停样式
    background: '#f0f0f0'
  }
}}>
  内容
</Box>
```

**常用 sx 属性：**
```typescript
sx={{
  // 间距
  p: 2,           // padding: 16px (2 * 8px)
  px: 2,          // padding-left, padding-right
  py: 2,          // padding-top, padding-bottom
  m: 2,           // margin: 16px
  gap: 2,         // flex gap: 16px
  
  // 尺寸
  width: 200,     // width: 200px
  minHeight: 100, // min-height: 100px
  
  // 布局
  display: 'flex',
  justifyContent: 'center',  // 水平居中
  alignItems: 'center',      // 垂直居中
  flexDirection: 'column',   // 垂直排列
  
  // 颜色
  color: 'text.primary',     // 使用主题文本颜色
  background: 'primary.main', // 使用主题主色
  
  // 圆角
  borderRadius: 2,  // 16px (2 * 8px)
}}
```

---

### 5.3 响应式设计

```typescript
<Stack 
  direction={{ xs: 'column', sm: 'row' }}  // 小屏竖排，大屏横排
  spacing={2}
>
  <Button>按钮1</Button>
  <Button>按钮2</Button>
</Stack>
```

**断点：**
- `xs` - 手机 (0px+)
- `sm` - 平板 (600px+)
- `md` - 笔记本 (900px+)
- `lg` - 台式机 (1200px+)

---

## 6. WebView2 通信机制

### 6.1 C# → React（发送消息）

**C# 代码：**
```csharp
// MainViewModel.cs
public void SendMessageToFrontend(string message) {
  var jsonMessage = JsonSerializer.Serialize(new {
    success = true,
    message = message,
    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
  });
  
  SendDataToFrontend?.Invoke(jsonMessage);  // 触发回调
}

// MainWindow.xaml.cs
MyWebView.CoreWebView2.PostWebMessageAsJson(jsonMessage);
```

**React 代码：**
```typescript
// Home.tsx
useEffect(() => {
  window.chrome.webview.addEventListener('message', (event) => {
    const data = JSON.parse(event.data);  // 解析 JSON
    console.log('收到消息:', data);
  });
}, []);
```

---

### 6.2 React → C#（发送消息）

**React 代码：**
```typescript
// Home.tsx
const sendToBackend = (action: string) => {
  const message = JSON.stringify({ action });
  window.chrome.webview.postMessage(message);
};
```

**C# 代码：**
```csharp
// MainWindow.xaml.cs
MyWebView.WebMessageReceived += (sender, args) => {
  string jsonMessage = args.WebMessageAsJson;
  var data = JsonSerializer.Deserialize<FrontendMessage>(jsonMessage);
  
  if (data.action == "start") {
    // 启动设备逻辑
  }
};
```

---

### 6.3 完整通信流程示例

**场景：点击"启动设备"按钮**

1. **用户点击按钮（React）**
   ```typescript
   <Button onClick={() => sendToBackend('start')}>
   ```

2. **发送消息到 C#（React）**
   ```typescript
   const sendToBackend = (action: string) => {
     window.chrome.webview.postMessage(JSON.stringify({ action }));
   };
   ```

3. **C# 接收并处理（C#）**
   ```csharp
   MyWebView.WebMessageReceived += (sender, args) => {
     var data = JsonSerializer.Deserialize<dynamic>(args.WebMessageAsJson);
     _viewModel.HandleFrontendMessage(data.action);  // 启动设备
   };
   ```

4. **C# 发送结果（C#）**
   ```csharp
   SendMessageToFrontend("设备启动成功");
   ```

5. **React 接收并显示（React）**
   ```typescript
   window.chrome.webview.addEventListener('message', (event) => {
     const data = JSON.parse(event.data);
     setLatestMessage(data.message);  // 显示 "设备启动成功"
   });
   ```

---

## 7. React Router 路由

### 7.1 路由配置（App.tsx）

```typescript
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'

function App() {
  return (
    <Router>
      <Navigation />  {/* 导航栏在所有页面都显示 */}
      
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/log" element={<Log />} />
        <Route path="/command" element={<Command />} />
      </Routes>
    </Router>
  )
}
```

---

### 7.2 导航链接（Navigation.tsx）

```typescript
import { Link, useLocation } from 'react-router-dom'

const Navigation = () => {
  const location = useLocation()  // 获取当前路径
  
  return (
    <Button 
      component={Link}  // 把 Button 转换为链接
      to="/"            // 目标路径
      sx={{
        color: location.pathname === '/' ? 'primary.main' : 'text.secondary'
      }}
    >
      Home
    </Button>
  )
}
```

**路由 Hooks：**
- `useLocation()` - 获取当前路径信息
- `useNavigate()` - 编程式导航

```typescript
import { useNavigate } from 'react-router-dom'

const MyComponent = () => {
  const navigate = useNavigate()
  
  const goToLog = () => {
    navigate('/log')  // 跳转到日志页
  }
  
  return <Button onClick={goToLog}>查看日志</Button>
}
```

---

## 8. 实战练习

### 练习 1：添加计数器组件

**目标：** 在 Home 页面添加一个简单计数器。

```typescript
// 在 Home.tsx 中添加
const [count, setCount] = useState<number>(0)

// 在 JSX 中添加
<Card>
  <CardContent>
    <Typography variant="h4">计数器: {count}</Typography>
    <Button onClick={() => setCount(count + 1)}>点击 +1</Button>
    <Button onClick={() => setCount(0)}>重置</Button>
  </CardContent>
</Card>
```

---

### 练习 2：添加输入框和状态

**目标：** 添加一个输入框，实时显示输入内容。

```typescript
// 添加状态
const [inputValue, setInputValue] = useState<string>('')

// 添加 JSX
<TextField
  label="输入设备名称"
  value={inputValue}
  onChange={(e) => setInputValue(e.target.value)}
/>
<Typography>你输入的是: {inputValue}</Typography>
```

---

### 练习 3：实现消息过滤

**目标：** 添加按钮，只显示成功/失败的消息。

```typescript
// 添加状态
const [filter, setFilter] = useState<'all' | 'success' | 'error'>('all')

// 过滤消息
const filteredMessages = messages.filter(msg => {
  if (filter === 'all') return true
  if (filter === 'success') return msg.success === true
  if (filter === 'error') return msg.success === false
  return true
})

// 添加按钮
<Button onClick={() => setFilter('all')}>全部</Button>
<Button onClick={() => setFilter('success')}>成功</Button>
<Button onClick={() => setFilter('error')}>失败</Button>

// 使用过滤后的消息
{filteredMessages.map((msg, index) => (
  <ListItem key={index}>...</ListItem>
))}
```

---

### 练习 4：创建自定义组件

**目标：** 把消息列表提取为独立组件。

```typescript
// 创建新文件：MessageList.tsx
interface MessageListProps {
  messages: BackendMessage[]
}

const MessageList = ({ messages }: MessageListProps) => {
  return (
    <List>
      {messages.map((msg, index) => (
        <ListItem key={index}>
          <ListItemText primary={msg.message} />
        </ListItem>
      ))}
    </List>
  )
}

export default MessageList

// 在 Home.tsx 中使用
import MessageList from '../components/MessageList'

<MessageList messages={messages} />
```

---

## 9. 常见错误和解决方法

### 错误 1：Cannot read property 'map' of undefined
```typescript
// ❌ 错误：messages 可能是 undefined
{messages.map(...)}

// ✅ 解决：使用可选链和默认值
{messages?.map(...) || <div>暂无消息</div>}
```

---

### 错误 2：Warning: Each child in a list should have a unique "key" prop
```typescript
// ❌ 忘记添加 key
{items.map(item => <div>{item}</div>)}

// ✅ 添加唯一 key
{items.map((item, index) => <div key={index}>{item}</div>)}
```

---

### 错误 3：状态更新不生效
```typescript
// ❌ 直接修改状态（不会触发重新渲染）
messages.push(newMessage)

// ✅ 使用 setState 创建新数组
setMessages([...messages, newMessage])
```

---

### 错误 4：useEffect 无限循环
```typescript
// ❌ 缺少依赖数组，每次渲染都执行
useEffect(() => {
  fetchData()
})

// ✅ 添加空依赖数组，只执行一次
useEffect(() => {
  fetchData()
}, [])
```

---

## 10. 学习资源

### 官方文档
- [React 官方文档](https://react.dev/) - 最权威的学习资源
- [TypeScript 官方文档](https://www.typescriptlang.org/)
- [Material UI 文档](https://mui.com/)

### 推荐视频教程
- B站搜索 "React 入门教程"
- B站搜索 "TypeScript 入门"

### 在线练习平台
- [CodeSandbox](https://codesandbox.io/) - 在线 React 编辑器
- [TypeScript Playground](https://www.typescriptlang.org/play) - 在线 TS 练习

---

## 11. 下一步学习路线

1. **巩固基础（1-2 周）**
   - 熟练使用 useState 和 useEffect
   - 理解 Props 和组件通信
   - 掌握列表渲染和条件渲染

2. **进阶学习（2-4 周）**
   - 学习 useContext（全局状态管理）
   - 学习 useReducer（复杂状态逻辑）
   - 学习自定义 Hooks
   - 学习表单处理（React Hook Form）

3. **项目实战（持续）**
   - 完善您的设备控制系统
   - 实现日志页面（Log.tsx）
   - 实现命令控制台（Command.tsx）
   - 实现配置页面（Config.tsx）

---

## 12. 快速参考卡片

### TypeScript 类型速查
```typescript
string, number, boolean, null, undefined
any (任意类型), void (无返回值)
Array<T> 或 T[]
{ key: type } (对象类型)
type | type (联合类型)
interface Name { ... } (接口)
```

### React Hooks 速查
```typescript
useState<T>(初始值)          // 状态管理
useEffect(() => {}, [依赖])   // 副作用
useContext(Context)          // 全局状态
useReducer(reducer, 初始值)   // 复杂状态
useRef<T>(初始值)            // 引用 DOM
useMemo(() => value, [依赖])  // 缓存值
useCallback(() => fn, [依赖]) // 缓存函数
```

### 数组方法速查
```typescript
.map(item => ...)     // 转换数组
.filter(item => ...)  // 过滤数组
.find(item => ...)    // 查找元素
.slice(start, end)    // 截取数组
.concat(...items)     // 合并数组
[...array, item]      // 扩展运算符
```

---

## 总结

恭喜！您已经掌握了：
- ✅ React 组件和 JSX 语法
- ✅ TypeScript 类型系统
- ✅ useState 和 useEffect Hooks
- ✅ Material UI 组件库
- ✅ WebView2 双向通信
- ✅ React Router 路由导航

**记住：** 编程是实践出来的，多写代码，遇到问题多查文档！

祝您学习愉快！🚀
