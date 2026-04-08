param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectFile = Join-Path $projectRoot 'Middle.csproj'
$publishDir = Join-Path $projectRoot 'publish'
$installerDir = Join-Path $projectRoot 'installer'

if (-not (Test-Path $projectFile)) {
    throw "未找到项目文件: $projectFile"
}

if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

Write-Host '==> 开始发布程序...' -ForegroundColor Cyan
& dotnet publish $projectFile -c $Configuration -r $RuntimeIdentifier --self-contained false -o $publishDir /p:InstallerAppVersion=$Version
if ($LASTEXITCODE -ne 0) {
    throw 'dotnet publish 执行失败。'
}

Write-Host ''
Write-Host '安装包生成完成。' -ForegroundColor Green
Write-Host "发布目录: $publishDir"
Write-Host "安装包目录: $installerDir"
Write-Host '注意：该安装包不包含 WebView2 Runtime 离线安装包，目标机器需预先安装 WebView2 Runtime 和对应的 .NET Desktop Runtime。'
