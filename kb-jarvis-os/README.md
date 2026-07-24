# KB Jarvis OS 12

**Developer:** KB (Khuda Bakhsh)  
**Product type:** Native Windows voice and task-execution assistant  
**Status:** Windows build validation in progress; no release package is published until compilation and executable verification pass

## What materially changed

Version 12 is not limited to a decorative dashboard. It adds:

- Gemini Live WebSocket voice sessions
- real microphone capture through NAudio
- streamed 16 kHz PCM input
- streamed 24 kHz audio playback
- English, Urdu, Roman Urdu and Hindi conversational instructions
- input and output transcription in the mission timeline
- Gemini text-agent fallback for typed commands
- local function calling and tool-result feedback
- Windows-DPAPI encryption for the Gemini API key
- native settings window
- automatic Live-session reconnect option
- Windows SAPI spoken fallback for typed replies
- deterministic local skill execution with result verification
- Version 12 Browser Companion and matching health handshake

## Locked identity

- Assistant: KB Jarvis
- Creator and developer: KB (Khuda Bakhsh)
- Primary user title: Boss
- Identity response: `Mujhe KB — Khuda Bakhsh ne design aur develop kiya hai. Main unka personal AI operating assistant hoon.`

## Current executable skills

- open supported Windows applications
- create, verify and open a Notepad note
- list existing Chrome tabs
- inspect the currently open WhatsApp Web chat
- type and verify a WhatsApp draft in the current chat
- request one confirmation before sending
- send and verify an outgoing WhatsApp message in the current chat
- open workspace, logs and training templates
- converse and plan through Gemini
- let Gemini call the verified local tools

Version 12 does not claim universal control over every program. Contact search, attachments, visual fallback and broad third-party application control still require dedicated verified skills.

## Installation

1. Extract the complete package.
2. Run `KB_Jarvis_OS.exe`.
3. Open `chrome://extensions/`.
4. Remove or reload the old KB Jarvis Browser Companion.
5. Enable Developer mode.
6. Choose **Load unpacked** and select the included `BrowserCompanion` folder.
7. Confirm Browser Companion version `12.0.0`.
8. In Jarvis, open **Gemini & Voice Settings**.
9. Save the Gemini API key and models.
10. Click **START LISTENING**.
11. Allow microphone access in Windows when required.

## Initial tests

Typed conversation:

```text
Hello Jarvis. Who created you?
```

Typed tool call:

```text
Open Calculator.
```

Voice:

```text
Jarvis, open Notepad and create a note saying Version 12 voice test is working.
```

WhatsApp current chat:

```text
In the current WhatsApp chat, draft: "KB Jarvis OS 12 test"
```

Send after one confirmation:

```text
In the current WhatsApp chat, send: "KB Jarvis OS 12 verified send test"
```

Then say or type:

```text
Haan, bhej do
```

## Important operational limits

- API quota and billing are controlled by the configured Google project.
- Account login, CAPTCHA and two-factor authentication cannot be bypassed.
- Windows can block injected input into higher-integrity or secure-desktop windows.
- A task is reported as completed only when the local skill returns verified success.
- `Close Jarvis` closes only Jarvis and never shuts down Windows.
