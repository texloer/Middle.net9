import { useEffect, useRef, useState } from 'react'
import type { ChangeEvent } from 'react'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  Container,
  FormControlLabel,
  MenuItem,
  Stack,
  Switch,
  TextField,
  Typography,
  alpha,
} from '@mui/material'
import SettingsIcon from '@mui/icons-material/Settings'
import SaveIcon from '@mui/icons-material/Save'

interface WebViewMessageEvent {
  data: string
}

interface ConfigForm {
  stationName: string
  deviceIp: string
  devicePort: string
  pollingIntervalMs: string
  autoStart: boolean
  theme: string
}

interface ConfigResponse {
  type?: string
  message?: string
  config?: Partial<ConfigForm>
  configPath?: string
  timestamp?: string
}

const defaultConfig: ConfigForm = {
  stationName: 'MidControl',
  deviceIp: '127.0.0.1',
  devicePort: '502',
  pollingIntervalMs: '1000',
  autoStart: false,
  theme: 'light',
}

const Config = () => {
  const [config, setConfig] = useState<ConfigForm>(defaultConfig)
  const [configPath, setConfigPath] = useState('')
  const [statusText, setStatusText] = useState('正在读取配置...')
  const [lastSavedAt, setLastSavedAt] = useState('')
  const [isSaving, setIsSaving] = useState(false)
  const saveTimerRef = useRef<number | null>(null)

  const sendToBackend = (payload: object) => {
    const webview = window.chrome?.webview
    if (!webview) {
      setStatusText('当前不在 WebView2 环境中，无法同步 config.ini')
      return
    }

    webview.postMessage(JSON.stringify(payload))
  }

  const normalizeConfig = (incoming?: Partial<ConfigForm>): ConfigForm => ({
    stationName: incoming?.stationName ?? defaultConfig.stationName,
    deviceIp: incoming?.deviceIp ?? defaultConfig.deviceIp,
    devicePort: incoming?.devicePort ?? defaultConfig.devicePort,
    pollingIntervalMs: incoming?.pollingIntervalMs ?? defaultConfig.pollingIntervalMs,
    autoStart: incoming?.autoStart ?? defaultConfig.autoStart,
    theme: incoming?.theme ?? defaultConfig.theme,
  })

  const persistConfig = (nextConfig: ConfigForm, isManualSave: boolean) => {
    setIsSaving(true)
    setStatusText(isManualSave ? '正在保存配置...' : '正在自动同步到 config.ini ...')

    sendToBackend({
      action: 'updateConfig',
      data: nextConfig,
    })
  }

  const scheduleAutoSave = (nextConfig: ConfigForm) => {
    if (saveTimerRef.current !== null) {
      window.clearTimeout(saveTimerRef.current)
    }

    saveTimerRef.current = window.setTimeout(() => {
      persistConfig(nextConfig, false)
    }, 300)
  }

  useEffect(() => {
    const handleMessage = (event: WebViewMessageEvent) => {
      try {
        const data: ConfigResponse = JSON.parse(event.data)
        if (!data.type || !data.type.startsWith('config')) {
          return
        }

        if (data.config) {
          setConfig(normalizeConfig(data.config))
        }

        if (data.configPath) {
          setConfigPath(data.configPath)
        }

        if (data.message) {
          setStatusText(data.message)
        }

        if (data.timestamp) {
          setLastSavedAt(data.timestamp)
        }

        setIsSaving(false)
      } catch (error) {
        console.error('解析配置消息失败:', error)
      }
    }

    const webview = window.chrome?.webview
    if (!webview) {
      setStatusText('当前不在 WebView2 环境中，无法读取 config.ini')
      return undefined
    }

    webview.addEventListener('message', handleMessage)
    webview.postMessage(JSON.stringify({ action: 'getConfig' }))

    return () => {
      webview.removeEventListener('message', handleMessage)

      if (saveTimerRef.current !== null) {
        window.clearTimeout(saveTimerRef.current)
      }
    }
  }, [])

  const handleTextChange =
    (field: keyof Omit<ConfigForm, 'autoStart'>) =>
    (event: ChangeEvent<HTMLInputElement>) => {
      const nextConfig = {
        ...config,
        [field]: event.target.value,
      }

      setConfig(nextConfig)
      scheduleAutoSave(nextConfig)
    }

  const handleAutoStartChange = (event: ChangeEvent<HTMLInputElement>) => {
    const nextConfig = {
      ...config,
      autoStart: event.target.checked,
    }

    setConfig(nextConfig)
    scheduleAutoSave(nextConfig)
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Card elevation={0}>
        <CardHeader 
          avatar={<SettingsIcon sx={{ fontSize: 32, color: 'primary.main' }} />}
          title="系统配置"
          subheader="管理设备参数和系统设置"
          titleTypographyProps={{ variant: 'h5', fontWeight: 700 }}
          sx={{ borderBottom: '1px solid', borderColor: alpha('#000', 0.08) }}
        />
        <CardContent>
          <Stack spacing={3}>
            <Alert severity="info" sx={{ borderRadius: 3 }}>
              <Typography variant="body2" fontWeight={600}>
                {statusText}
              </Typography>
              {configPath && (
                <Typography variant="caption" display="block" sx={{ mt: 0.5 }}>
                  配置文件：{configPath}
                </Typography>
              )}
            </Alert>

            <Box
              sx={{
                p: 3,
                borderRadius: 3,
                background: alpha('#e0e7ff', 0.24),
                border: '1px solid',
                borderColor: alpha('#6366f1', 0.12),
              }}
            >
              <Stack spacing={3}>
                <TextField
                  label="站点名称"
                  value={config.stationName}
                  onChange={handleTextChange('stationName')}
                  fullWidth
                />

                <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
                  <TextField
                    label="设备 IP"
                    value={config.deviceIp}
                    onChange={handleTextChange('deviceIp')}
                    fullWidth
                  />
                  <TextField
                    label="设备端口"
                    value={config.devicePort}
                    onChange={handleTextChange('devicePort')}
                    type="number"
                    fullWidth
                  />
                </Stack>

                <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
                  <TextField
                    label="轮询间隔 (ms)"
                    value={config.pollingIntervalMs}
                    onChange={handleTextChange('pollingIntervalMs')}
                    type="number"
                    fullWidth
                  />
                  <TextField
                    label="主题"
                    value={config.theme}
                    onChange={handleTextChange('theme')}
                    select
                    fullWidth
                  >
                    <MenuItem value="light">Light</MenuItem>
                    <MenuItem value="dark">Dark</MenuItem>
                  </TextField>
                </Stack>

                <FormControlLabel
                  control={<Switch checked={config.autoStart} onChange={handleAutoStartChange} />}
                  label="启动应用时自动连接设备"
                />
              </Stack>
            </Box>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ xs: 'stretch', sm: 'center' }}>
              <Button
                variant="contained"
                startIcon={<SaveIcon />}
                onClick={() => persistConfig(config, true)}
                disabled={isSaving}
              >
                保存到 config.ini
              </Button>

              <Chip
                color={isSaving ? 'warning' : 'success'}
                label={lastSavedAt ? `最近同步：${lastSavedAt}` : '等待首次保存'}
              />
            </Stack>
          </Stack>
        </CardContent>
      </Card>
    </Container>
  )
}

export default Config
