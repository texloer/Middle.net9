# 启动 Vite 开发服务器（支持热更新）
Write-Host "🚀 启动 Vite 开发服务器..." -ForegroundColor Cyan
Write-Host "📝 修改 App.tsx 后会自动刷新，无需重启程序！" -ForegroundColor Green
Write-Host ""

Set-Location my-ui
npm run dev
