import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  // 1. 确保是相对路径，解决 WebView2 加载本地文件的路径问题
  base: './', 
  build: {
    // 2. 配置打包输出目录
    // __dirname 指的是当前文件 (vite.config.ts) 所在的目录，即 my-ui
    // '../WebAsset' 表示向上退一级，进入 WebAsset 文件夹
    outDir: path.resolve(__dirname, '../WebAsset'),
    
    // 3. 每次打包清空旧文件，防止旧代码干扰
    emptyOutDir: true,
  }
})