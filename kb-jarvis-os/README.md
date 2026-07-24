# KB Jarvis OS v11

**Developer:** KB (Khuda Bakhsh)  
**Product type:** Native Windows AI operating environment  
**Status:** Native milestone under active build validation

## Product goal

KB Jarvis OS is a native Windows desktop assistant that understands the Boss's goal, selects a trained local skill, executes the complete workflow, verifies the result, retries through alternate methods when necessary, and asks the Boss only for consequential approval, authentication, or genuinely missing information.

## Locked product identity

- Assistant: KB Jarvis
- Creator and developer: KB (Khuda Bakhsh)
- Primary user title: Boss
- Identity response: "Mujhe KB — Khuda Bakhsh ne design aur develop kiya hai. Main unka personal AI operating assistant hoon."

## Implemented in the current branch

- Native WPF Windows executable rather than an Edge app window
- Animated black, cyan and red futuristic command centre
- Native title bar, minimize, maximize and safe close behaviour
- Local deterministic skill registry
- Native Windows `SendInput` and foreground-window bridge
- Notepad write, disk verification and Notepad review skill
- Embedded localhost WebSocket Browser Companion server
- Chrome extension with automatic reconnect and no recurring pair code
- WhatsApp current-chat inspection and chat-header evidence
- WhatsApp current-chat exact draft verification
- One-time confirmation queue for message sending
- Outgoing WhatsApp message verification
- Permanent KB (Khuda Bakhsh) identity in backend and UI
- Windows x64 self-contained single-file EXE workflow

## Acceptance commands

```text
Who created you?
```

```text
Open Notepad and write: "KB Jarvis OS native Notepad skill is working."
```

```text
In the current WhatsApp chat, draft: "KB Jarvis OS test message"
```

```text
In the current WhatsApp chat, send: "KB Jarvis OS verified send test"
```

Then confirm once:

```text
Haan, bhej do
```

## Browser Companion installation

1. Open `chrome://extensions/`.
2. Enable Developer mode.
3. Choose **Load unpacked**.
4. Select the `kb-jarvis-os/browser-companion` folder.
5. Start `KB_Jarvis_OS.exe`.
6. The extension scans local ports 32145–32155 and reconnects automatically.

## Development rule

A click, keystroke or tool call is not success. A task is complete only when the intended result has been verified.

## Next acceptance-critical work

- WhatsApp contact search and candidate disambiguation
- Attachment upload and verified sending
- Existing Notepad editor typing mode
- System-tray and floating-orb worker mode
- Teach Mode recording and versioned skill memory
- Native screenshot and visual fallback engine
