import { Container, Typography, Card, CardHeader, CardContent, Box, alpha } from '@mui/material'
import TerminalIcon from '@mui/icons-material/Terminal'

const Command = () => {
  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Card elevation={0}>
        <CardHeader 
          avatar={<TerminalIcon sx={{ fontSize: 32, color: 'primary.main' }} />}
          title="命令控制台"
          subheader="执行设备命令和脚本"
          titleTypographyProps={{ variant: 'h5', fontWeight: 700 }}
          sx={{ borderBottom: '1px solid', borderColor: alpha('#000', 0.08) }}
        />
        <CardContent>
          <Box sx={{ 
            textAlign: 'center', 
            py: 12,
            borderRadius: 2,
            background: alpha('#e0e7ff', 0.3),
          }}>
            <TerminalIcon sx={{ fontSize: 80, color: 'text.disabled', mb: 3 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>
              命令控制台开发中...
            </Typography>
            <Typography variant="body2" color="text.secondary">
              即将支持命令输入、历史记录、自动补全等功能
            </Typography>
          </Box>
        </CardContent>
      </Card>
    </Container>
  )
}

export default Command
