import { useState, useEffect, useCallback } from 'react'
import {
  Box,
  Container,
  Typography,
  Button,
  Card,
  CardContent,
  CardHeader,
  List,
  ListItem,
  ListItemText,
  Chip,
  Stack,
  Alert,
  alpha
} from '@mui/material'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import StopIcon from '@mui/icons-material/Stop'
import SearchIcon from '@mui/icons-material/Search'
import MessageIcon from '@mui/icons-material/Message'

interface BackendMessage {
  type?: string;
  page?: string;
  success?: boolean;
  message?: string;
  timestamp?: string;
  status?: string;
  temperature?: number;
  isRunning?: boolean;
  deviceStatus?: string;
}

const Home = () => {
  const [messages, setMessages] = useState<BackendMessage[]>([])
  const [latestMessage, setLatestMessage] = useState<string>('')
  const [deviceStatus, setDeviceStatus] = useState<string>('未连接')
  const [isRunning, setIsRunning] = useState<boolean>(false)

  // 格式化消息用于 UI 显示
  const formatMessage = (m: BackendMessage) => {
    if (m.message) return m.message
    if (m.type === 'deviceStatus') {
      const parts: string[] = []
      if (m.deviceStatus || m.status) parts.push(`状态: ${m.deviceStatus || m.status}`)
      if (m.isRunning !== undefined) parts.push(m.isRunning ? '运行中' : '未运行')
      if (m.temperature !== undefined) parts.push(`温度: ${m.temperature}°C`)
      const combined = parts.join(' | ')
      return combined || JSON.stringify(m, null, 2)
    }
    // 其它类型：尽量返回可读字符串，否则返回美化 JSON
    return JSON.stringify(m, null, 2)
  }

  // 向 C# 后端发送消息
  const sendToBackend = useCallback((action: string, data?: unknown) => {
    const webview = window.chrome?.webview;
    if (!webview) {
      console.warn('不在 WebView2 环境中');
      alert('不在 WebView2 环境中，无法发送消息');
      return;
    }

    const message = {
      page: 'home',
      action,
      data,
      timestamp: new Date().toISOString()
    };
    
    console.log('Home 发送消息:', message);
    webview.postMessage(JSON.stringify(message));
  }, []);

  // 监听来自 C# 后端的消息
  useEffect(() => {
    const webview = window.chrome?.webview;
    if (!webview) {
      console.warn('WebView2 不可用');
      return;
    }

    const handleMessage = (event: MessageEvent) => {
      try {
        const data: BackendMessage = typeof event.data === 'string' ? JSON.parse(event.data) : (event.data as BackendMessage);
        
        // 只处理发给 home 页面的消息
        if (data.page === 'home' || !data.page) {
          console.log('Home 收到消息:', data);
          
          // 更新消息历史
          setMessages(prev => [...prev, data].slice(-10));
          setLatestMessage(data.message || '');

          // 更新设备状态
          if (data.type === 'deviceStatus') {
            setDeviceStatus(data.deviceStatus || data.status || '未知');
            setIsRunning(data.isRunning ?? false);
          }
        }
      } catch (error) {
        console.error('解析消息失败:', error);
        if (typeof event.data === 'string') {
          setLatestMessage(`原始消息: ${event.data}`);
        }
      }
    };

    webview.addEventListener('message', handleMessage);

    // 组件挂载时查询设备状态
    sendToBackend('query');

    // 清理函数
    return () => {
      webview.removeEventListener('message', handleMessage);
    };
  }, [sendToBackend]);

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {/* 设备状态横幅 */}
      <Alert 
        severity={isRunning ? 'success' : 'info'}
        icon={<MessageIcon />}
        sx={{ 
          mb: 3,
          backdropFilter: 'blur(20px)',
          backgroundColor: isRunning 
            ? 'rgba(16, 185, 129, 0.08)' 
            : 'rgba(99, 102, 241, 0.08)',
          border: '1px solid',
          borderColor: isRunning 
            ? alpha('#10b981', 0.3) 
            : alpha('#6366f1', 0.3),
          borderRadius: 3,
        }}
      >
        <Typography variant="body2" fontWeight={500}>
          <strong>设备状态:</strong> {deviceStatus} 
          {latestMessage && ` | ${latestMessage}`}
        </Typography>
      </Alert>

      <Stack spacing={3}>
        {/* 控制按钮区 */}
        <Card elevation={0}>
          <CardHeader 
            title="设备控制面板"
            titleTypographyProps={{ variant: 'h6', fontWeight: 600 }}
            sx={{ borderBottom: '1px solid', borderColor: alpha('#000', 0.08) }}
          />
          <CardContent sx={{ py: 4 }}>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} justifyContent="center">
              <Button
                variant="contained"
                size="large"
                startIcon={<PlayArrowIcon />}
                onClick={() => sendToBackend('start')}
                disabled={isRunning}
                sx={{ 
                  minWidth: 160,
                  borderRadius: 4,
                  py: 1.5,
                  background: isRunning 
                    ? 'linear-gradient(135deg, #9ca3af 0%, #6b7280 100%)'
                    : 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)',
                  '&:hover': {
                    background: isRunning 
                      ? 'linear-gradient(135deg, #9ca3af 0%, #6b7280 100%)'
                      : 'linear-gradient(135deg, #818cf8 0%, #6366f1 100%)',
                    boxShadow: isRunning 
                      ? 'none'
                      : '0 6px 24px 0 rgba(99, 102, 241, 0.6)',
                  },
                }}
              >
                启动设备
              </Button>
              <Button
                variant="contained"
                size="large"
                startIcon={<StopIcon />}
                onClick={() => sendToBackend('stop')}
                disabled={!isRunning}
                sx={{ 
                  minWidth: 160,
                  borderRadius: 4,
                  py: 1.5,
                  background: !isRunning 
                    ? 'linear-gradient(135deg, #9ca3af 0%, #6b7280 100%)'
                    : 'linear-gradient(135deg, #f43f5e 0%, #e11d48 100%)',
                  '&:hover': {
                    background: !isRunning 
                      ? 'linear-gradient(135deg, #9ca3af 0%, #6b7280 100%)'
                      : 'linear-gradient(135deg, #fb7185 0%, #f43f5e 100%)',
                    boxShadow: !isRunning 
                      ? 'none'
                      : '0 6px 24px 0 rgba(244, 63, 94, 0.6)',
                  },
                }}
              >
                停止设备
              </Button>
              <Button
                variant="contained"
                size="large"
                startIcon={<SearchIcon />}
                onClick={() => sendToBackend('query')}
                sx={{ 
                  minWidth: 160,
                  borderRadius: 4,
                  py: 1.5,
                  background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
                  '&:hover': {
                    background: 'linear-gradient(135deg, #34d399 0%, #10b981 100%)',
                    boxShadow: '0 6px 24px 0 rgba(16, 185, 129, 0.5)',
                  },
                }}
              >
                查询状态
              </Button>
            </Stack>
          </CardContent>
        </Card>

        {/* 消息历史记录 */}
        <Card elevation={0}>
          <CardHeader 
            title="实时消息监控"
            subheader="来自 C# 后端的实时数据流 (最多显示10条)"
            titleTypographyProps={{ variant: 'h6', fontWeight: 600 }}
            sx={{ borderBottom: '1px solid', borderColor: alpha('#000', 0.08) }}
          />
          <CardContent>
            {messages.length === 0 ? (
              <Box sx={{ textAlign: 'center', py: 8, borderRadius: 2, background: alpha('#e0e7ff', 0.5) }}>
                <MessageIcon sx={{ fontSize: 64, color: 'text.disabled', mb: 2 }} />
                <Typography variant="h6" color="text.secondary" gutterBottom>
                  暂无消息
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  点击上方控制按钮开始与后端通信
                </Typography>
              </Box>
            ) : (
              <List sx={{ maxHeight: 450, overflow: 'auto' }}>
                {messages.map((msg, index) => (
                  <ListItem 
                    key={index}
                    sx={{ 
                      flexDirection: 'column',
                      alignItems: 'flex-start',
                      gap: 1.5,
                      py: 2,
                      px: 3,
                      borderRadius: 2,
                      mb: 1,
                      background: alpha('#e0e7ff', 0.3),
                      border: '1px solid',
                      borderColor: alpha('#a855f7', 0.15),
                    }}
                  >
                    <Box sx={{ display: 'flex', gap: 1, width: '100%', flexWrap: 'wrap' }}>
                      <Chip 
                        label={msg.timestamp || '无时间'} 
                        size="small" 
                        variant="outlined"
                      />
                      {msg.success !== undefined && (
                        <Chip 
                          label={msg.success ? '✓ 成功' : '✗ 失败'} 
                          size="small"
                          color={msg.success ? 'success' : 'error'}
                        />
                      )}
                      {msg.type && (
                        <Chip 
                          label={msg.type} 
                          size="small" 
                          color="primary"
                          variant="outlined"
                        />
                      )}
                      {msg.temperature !== undefined && (
                        <Chip 
                          label={`温度: ${msg.temperature}°C`} 
                          size="small" 
                          color="info"
                        />
                      )}
                    </Box>
                    <ListItemText 
                      primary={
                        <Typography
                          variant="body2"
                          component="div"
                          sx={{ fontWeight: 500, color: 'text.primary', whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}
                        >
                          {formatMessage(msg)}
                        </Typography>
                      }
                      primaryTypographyProps={{ 
                        // 这里可保留一些样式控制（如果需要）
                      }}
                    />
                  </ListItem>
                ))}
              </List>
            )}
          </CardContent>
        </Card>
      </Stack>
    </Container>
  )
}

export default Home
