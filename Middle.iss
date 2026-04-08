#ifndef MyAppName
  #define MyAppName "Middle"
#endif

#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif

#ifndef MyAppPublisher
  #define MyAppPublisher "SemiconductorControlSystem"
#endif

#ifndef MyAppExeName
  #define MyAppExeName "Middle.exe"
#endif

#ifndef PublishDir
  #define PublishDir "publish"
#endif

[Setup]
AppId={{7E6D8A59-2C3E-4D0D-94F9-3C0134AF8B39}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=installer
OutputBaseFilename=MiddleSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
AppComments=Requires .NET Desktop Runtime and Microsoft Edge WebView2 Runtime to be installed on the target machine.

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加任务："; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "启动 {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
procedure InitializeWizard;
begin
  SuppressibleMsgBox(
    '此安装包不会内置 WebView2 Runtime 离线安装包。'#13#10#13#10 +
    '安装前请确认目标工控机已安装：'#13#10 +
    '1. Microsoft Edge WebView2 Runtime'#13#10 +
    '2. 对应版本的 .NET Desktop Runtime',
    mbInformation,
    MB_OK,
    IDOK);
end;
