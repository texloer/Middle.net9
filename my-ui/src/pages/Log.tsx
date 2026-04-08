import { Container, Typography, Card, CardHeader, CardContent, Box, alpha } from '@mui/material'
import ArticleIcon from '@mui/icons-material/Article'

const Log = () => {
  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Card elevation={0}>
        <CardHeader 
          avatar={<ArticleIcon sx={{ fontSize: 32, color: 'primary.main' }} />}
          title="系统日志"
          subheader="查看系统运行日志和历史记录"
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
            <ArticleIcon sx={{ fontSize: 80, color: 'text.disabled', mb: 3 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>
              日志功能开发中...
            </Typography>
            <Typography variant="body2" color="text.secondary">
              即将支持实时日志查看、筛选、导出等功能
            </Typography>
          </Box>
        </CardContent>
      </Card>
    </Container>
  )
}

export default Log
