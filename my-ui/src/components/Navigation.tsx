import { AppBar, Toolbar, Typography, Button, Box, alpha } from '@mui/material'
import { Link, useLocation } from 'react-router-dom'
import HomeIcon from '@mui/icons-material/Home'
import ArticleIcon from '@mui/icons-material/Article'
import TerminalIcon from '@mui/icons-material/Terminal'
import SettingsIcon from '@mui/icons-material/Settings'
import MemoryIcon from '@mui/icons-material/Memory'
import InputIcon from '@mui/icons-material/Input'
import InfoIcon from '@mui/icons-material/Info'

const Navigation = () => {
  const location = useLocation()

  const navItems = [
    { path: '/', label: 'Home', icon: <HomeIcon /> },
    { path: '/log', label: 'Log', icon: <ArticleIcon /> },
    { path: '/command', label: 'Command', icon: <TerminalIcon /> },
    { path: '/io', label: 'IO', icon: <InputIcon /> },
    { path: '/config', label: 'Config', icon: <SettingsIcon /> },
    { path: '/info', label: 'Info Pad', icon: <InfoIcon /> },
  ]

  return (
    <AppBar 
      position="sticky" 
      elevation={0}
      sx={{ 
        background: 'rgba(255, 255, 255, 0.8)',
        backdropFilter: 'blur(20px)',
        borderBottom: '1px solid',
        borderColor: alpha('#6366f1', 0.2),
      }}
    >
      <Toolbar>
        {/* Logo */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mr: 4 }}>
          <MemoryIcon sx={{ fontSize: 32, color: 'primary.main' }} />
          <Typography 
            variant="h6" 
            sx={{ 
              background: 'linear-gradient(135deg, #6366f1 0%, #a855f7 100%)',
              backgroundClip: 'text',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              fontWeight: 700,
            }}
          >
            MidControl
          </Typography>
        </Box>

        {/* Navigation - 靠左对齐 */}
        <Box sx={{ display: 'flex', gap: 1, flex: 1 }}>
          {navItems.map((item) => (
            <Button
              key={item.path}
              component={Link}
              to={item.path}
              startIcon={item.icon}
              sx={{
                color: location.pathname === item.path ? 'primary.main' : 'text.secondary',
                fontWeight: location.pathname === item.path ? 600 : 500,
                px: 2,
                py: 1,
                borderRadius: 2,
                background: location.pathname === item.path 
                  ? alpha('#6366f1', 0.1) 
                  : 'transparent',
                '&:hover': {
                  background: alpha('#6366f1', 0.08),
                },
              }}
            >
              {item.label}
            </Button>
          ))}
        </Box>
      </Toolbar>
    </AppBar>
  )
}

export default Navigation
