import { Container, Typography, Card, CardHeader, CardContent, Box, alpha } from '@mui/material'
import InputIcon from '@mui/icons-material/Input'

const IO = () => {
  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Card elevation={0}>
        <CardHeader 
          avatar={<InputIcon sx={{ fontSize: 32, color: 'primary.main' }} />}
          title="IO 监控"
          subheader="输入输出信号监控与控制"
          titleTypographyProps={{ variant: 'h5', fontWeight: 700 }}
          sx={{ borderBottom: '1px solid', borderColor: alpha('#000', 0.08) }}
        />
        <CardContent>
          <Box sx={{ 
            textAlign: 'center', 
            py: 12, 
            borderRadius: 2, 
            background: alpha('#e0e7ff', 0.3) 
          }}>
            <InputIcon sx={{ fontSize: 80, color: 'text.disabled', mb: 3 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>
              IO 功能开发中...
            </Typography>
            <Typography variant="body2" color="text.secondary">
              即将支持数字量/模拟量监控、IO 配置等功能
            </Typography>
          </Box>
        </CardContent>
      </Card>
    </Container>
  )
}

export default IO
