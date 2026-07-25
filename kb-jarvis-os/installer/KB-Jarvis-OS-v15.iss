#define MyAppName "KB Jarvis OS"
#define MyAppVersion "15.0.1"
#define MyAppPublisher "KB (Khuda Bakhsh)"
#define MyAppExeName "KB_Jarvis_OS.exe"

[Setup]
AppId={{9CB1144E-1DBA-47A5-A1D1-89A05A1EF315}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\KB Jarvis OS
DefaultGroupName=KB Jarvis OS
DisableProgramGroupPage=yes
OutputDir=..\..\artifacts\installer
OutputBaseFilename=KB-Jarvis-OS-v15-Setup
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupLogging=yes
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\..\artifacts\KB-Jarvis-OS-v15\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\KB Jarvis OS"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\KB Jarvis OS"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon
Name: "{autoprograms}\KB Jarvis OS Browser Companion"; Filename: "{app}\BrowserCompanion"; WorkingDir: "{app}\BrowserCompanion"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: checkedonce

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch KB Jarvis OS"; Flags: nowait postinstall skipifsilent
Filename: "explorer.exe"; Parameters: "{app}\BrowserCompanion"; Description: "Open the Browser Companion folder"; Flags: postinstall skipifsilent unchecked

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
  if not IsWin64 then
  begin
    MsgBox('KB Jarvis OS 15 requires 64-bit Windows 10 or Windows 11.', mbError, MB_OK);
    Result := False;
  end;
end;
