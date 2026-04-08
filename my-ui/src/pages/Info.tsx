import { Container, Typography, Card, CardHeader, CardContent, Box, alpha } from '@mui/material'
import InfoIcon from '@mui/icons-material/Info'

const Info = () => {
  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Card elevation={0}>
        <CardHeader 
          avatar={<InfoIcon sx={{ fontSize: 32, color: 'primary.main' }} />}
          title="系统信息"
          subheader="查看系统状态、版本信息及运行数据"
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
            <InfoIcon sx={{ fontSize: 80, color: 'text.disabled', mb: 3 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>
              Info 功能开发中...
            </Typography>
            <Typography variant="body2" color="text.secondary">
              即将支持系统信息展示、版本管理、运行统计等功能
            </Typography>
          </Box>
        </CardContent>
      </Card>
    </Container>
  )
}

export default Info
