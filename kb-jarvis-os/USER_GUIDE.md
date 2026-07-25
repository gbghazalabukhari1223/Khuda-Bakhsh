# KB Jarvis OS 15 User Guide

## Install with the setup program

1. Close every previous KB Jarvis process.
2. Run `KB-JARVIS-Setup-x64.exe`.
3. Approve the Windows administrator prompt.
4. Keep the default installation folder unless another location is required.
5. Select the Desktop shortcut option if desired.
6. Finish installation and start KB Jarvis OS.

The installer creates Start-menu and optional Desktop shortcuts and registers an uninstaller in Windows Apps & Features.

## Portable use

The portable ZIP does not install the product. Extract the complete ZIP to a fresh folder and run `KB_Jarvis_OS.exe`. Do not run it from inside the ZIP.

## Browser Companion

1. Open `chrome://extensions/` or `edge://extensions/`.
2. Enable Developer mode.
3. Remove or disable an older KB Jarvis Browser Companion.
4. Select Load unpacked.
5. Choose the installed or extracted `BrowserCompanion` folder.
6. Confirm extension version `15.0.0`.
7. Start Jarvis and wait for the companion to report connected.

## Gemini and female voice

1. Open Gemini & Voice Settings.
2. Enter the Gemini API key.
3. Keep the recommended Live model and text planner unless a supported replacement is deliberately selected.
4. Select `Laomedeia` for the default professional female Live voice.
5. Save settings.
6. Press Start Listening.

Voice states include CONNECTING, RESUMING, CONNECTED, LISTENING, RECONNECTING and OFFLINE.

## Screen and camera

Use Start Screen or Start Camera only when visual context is needed. Each input has its own Stop control. Keep private passwords, banking details and confidential documents away from the visible screen or camera while sharing is enabled.

## Commands

### Multiple tasks

```text
Open Calculator, then play Afreen Afreen on YouTube,
then create a Desktop folder named Website Work,
and finally list my browser tabs.
```

Jarvis executes supported tasks in order and verifies each result before starting the next. A batch pauses when a consequential step needs approval.

### Files and folders

```text
Create a folder named Website Work on my Desktop.
```

```text
Meray PC par Notepad wali sab .txt files ko Desktop par
Organized Notepad Files folder mein move kar do.
```

The safe `PC` search scope includes Desktop, Documents and Downloads. Moving files requires confirmation.

### WhatsApp

Keep an existing signed-in WhatsApp Web tab open.

```text
Send "Meeting is tomorrow at 12" to Zain on WhatsApp.
```

Jarvis searches inside the existing session, verifies the chat and draft, requests confirmation, sends and verifies the outgoing message.

### YouTube

```text
Open YouTube and play Pasoori Coke Studio.
```

Jarvis searches, opens a normal video result and checks whether the video is actually playing.

### Website Studio

```text
Create a responsive HTML CSS JavaScript website project named Fashion Studio.
```

```text
In Fashion Studio create pages/about.html titled About the Designer.
```

```text
Update css/style.css in Fashion Studio with this complete CSS: ...
```

```text
Validate the Fashion Studio website project.
```

Projects are normally stored in `Documents\KB Jarvis Websites`. Existing files receive timestamped backups before replacement.

### WordPress

Keep an existing signed-in WordPress admin tab open.

```text
Create a WordPress draft post titled Latest Pakistani Fashion Trends
and write a complete SEO-friendly article.
```

Publishing requires confirmation. Custom builders may require screen vision.

## Logs and troubleshooting

Logs are stored under:

```text
%LOCALAPPDATA%\KB Jarvis OS\Logs
```

For startup problems, use PowerShell from the installation folder:

```powershell
$p = Start-Process ".\KB_Jarvis_OS.exe" -PassThru -Wait
$p.ExitCode
```

Then inspect recent .NET and Application errors:

```powershell
Get-WinEvent -FilterHashtable @{
  LogName='Application'
  StartTime=(Get-Date).AddMinutes(-10)
} | Where-Object {
  $_.Message -match 'KB_Jarvis_OS|Application Error|.NET Runtime'
} | Format-List TimeCreated,ProviderName,Id,Message
```

## Uninstall

Open Windows Settings → Apps → Installed apps → KB Jarvis OS → Uninstall. The uninstaller removes installed program files and shortcuts. Local settings and logs under `%LOCALAPPDATA%\KB Jarvis OS` are retained to prevent accidental loss; they can be removed manually after backing up anything needed.

## Safety boundaries

Jarvis does not bypass passwords, CAPTCHA, two-factor authentication or UAC secure desktop. Sending messages, publishing content, moving files and other consequential actions require confirmation. Close Jarvis closes only the Jarvis application and does not shut down Windows.
