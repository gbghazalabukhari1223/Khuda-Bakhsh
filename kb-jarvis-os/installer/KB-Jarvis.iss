#define MyAppName "KB Jarvis OS"
#define MyAppVersion "15.0.0"
#define MyAppPublisher "KB (Khuda Bakhsh)"
#define MyAppExeName "KB_Jarvis_OS.exe"

[Setup]
AppId={{B9F9D6FD-1C7E-4ED6-981C-95FA176D5C15}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Professional female multimodal Windows, browser and website work assistant
DefaultDirName={autopf}\KB Jarvis OS
DefaultGroupName=KB Jarvis OS
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\..\artifacts\release
OutputBaseFilename=KB-JARVIS-Setup-x64
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
SetupLogging=yes
CloseApplications=yes
RestartApplications=no
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName=KB Jarvis OS 15
ChangesAssociations=no
ChangesEnvironment=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a Desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "..\..\artifacts\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\KB Jarvis OS"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{group}\User Guide"; Filename: "{app}\USER_GUIDE.md"
Name: "{autodesktop}\KB Jarvis OS"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch KB Jarvis OS"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\BrowserCompanion"

[Code]
function InitializeSetup(): Boolean;
begin
  Result := IsWin64;
  if not Result then
    MsgBox('KB Jarvis OS 15 requires 64-bit Windows.', mbError, MB_OK);
end;
