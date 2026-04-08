import { BrowserRouter as Router, Navigate, Route, Routes } from 'react-router-dom'
import { 
  ThemeProvider, 
  createTheme, 
  CssBaseline,
  Box,
  Typography,
  alpha
} from '@mui/material'
import Navigation from './components/Navigation'
import Home from './pages/Home'
import Log from './pages/Log'
import Command from './pages/Command'
import IO from './pages/IO'
import Config from './pages/Config'
import Info from './pages/Info'

// 高级浅色主题配置 - 清新优雅
const premiumTheme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#6366f1',
      light: '#818cf8',
      dark: '#4f46e5',
    },
    secondary: {
      main: '#a855f7',
      light: '#c084fc',
      dark: '#9333ea',
    },
    success: {
      main: '#10b981',
      light: '#34d399',
      dark: '#059669',
    },
    error: {
      main: '#f43f5e',
      light: '#fb7185',
      dark: '#e11d48',
    },
    background: {
      default: '#f8fafc',
      paper: 'rgba(255, 255, 255, 0.9)',
    },
    text: {
      primary: '#1e293b',
      secondary: '#64748b',
    },
  },
  typography: {
    fontFamily: '"Inter", "Segoe UI", "Microsoft YaHei", sans-serif',
    h4: { fontWeight: 700, letterSpacing: '-0.02em' },
    h5: { fontWeight: 700, letterSpacing: '-0.015em' },
    h6: { fontWeight: 600, letterSpacing: '-0.01em' },
  },
  shape: {
    borderRadius: 16,
  },
  components: {
    MuiCard: {
      styleOverrides: {
        root: {
          backdropFilter: 'blur(20px)',
          backgroundColor: 'rgba(255, 255, 255, 0.8)',
          border: '1px solid rgba(99, 102, 241, 0.15)',
          boxShadow: '0 4px 24px 0 rgba(99, 102, 241, 0.12)',
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          fontWeight: 600,
          borderRadius: 12,
          padding: '10px 24px',
        },
      },
    },
  },
});

function App() {
  return (
    <ThemeProvider theme={premiumTheme}>
      <CssBaseline />
      <Router>
        <Box sx={{ 
          minHeight: '100vh',
          background: 'linear-gradient(135deg, #f0f9ff 0%, #e0e7ff 50%, #f0f9ff 100%)',
          position: 'relative',
          overflow: 'hidden',
          '&::before': {
            content: '""',
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            background: 'radial-gradient(circle at 20% 50%, rgba(99, 102, 241, 0.08) 0%, transparent 50%), radial-gradient(circle at 80% 80%, rgba(168, 85, 247, 0.08) 0%, transparent 50%)',
            pointerEvents: 'none',
          },
        }}>
          {/* 导航栏 */}
          <Navigation />

          {/* 路由内容 */}
          <Box sx={{ position: 'relative', zIndex: 1 }}>
            <Routes>
              <Route path="/index.html" element={<Navigate to="/" replace />} />
              <Route path="/" element={<Home />} />
              <Route path="/log" element={<Log />} />
              <Route path="/command" element={<Command />} />
              <Route path="/io" element={<IO />} />
              <Route path="/config" element={<Config />} />
              <Route path="/info" element={<Info />} />
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </Box>

          {/* 底部信息 */}
          <Box sx={{ py: 4, textAlign: 'center', position: 'relative', zIndex: 1 }}>
            <Typography 
              variant="caption" 
              sx={{ 
                color: alpha('#000', 0.5),
                fontWeight: 500,
                letterSpacing: '0.5px',
              }}
            >
              Powered by React + Material UI + WebView2
            </Typography>
          </Box>
        </Box>
      </Router>
    </ThemeProvider>
  )
}

export default App
