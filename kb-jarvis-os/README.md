# KB Jarvis OS 13

**Developer:** KB (Khuda Bakhsh)  
**Product type:** Native Windows multimodal voice and visual task-execution assistant  
**Status:** Compiled Windows milestone with verified native build and Version 13 Browser Companion

## What materially changed

Version 13 adds:

- stronger English, Urdu, Roman Urdu and Hindi intent handling
- Gemini Live voice with local function calling
- optional live screen vision on/off
- optional live camera vision on/off
- one visual frame per second supplied to Live voice
- current screen/camera images supplied to typed AI tasks
- normalized native mouse and keyboard visual actions
- responsive wide, medium and compact dashboard layouts
- local file and folder search across standard user locations
- Windows taskbar/Start search and optional top-result opening
- existing-session-only WhatsApp operation
- WhatsApp contact search, candidate ranking and chat-header verification
- verified WhatsApp draft and one-confirmation send workflow
- automatic Browser Companion reconnect
- Windows-DPAPI encryption for the Gemini API key
- permanent creator identity: KB (Khuda Bakhsh)

## Locked identity

- Assistant: KB Jarvis
- Creator and developer: KB (Khuda Bakhsh)
- Primary user title: Boss
- Identity response: `Mujhe KB — Khuda Bakhsh ne design aur develop kiya hai. Main unka personal multimodal Windows operating assistant hoon.`

## Important WhatsApp rule

Jarvis does not create, refresh or navigate to a new WhatsApp Web tab for messaging. It searches for an existing `web.whatsapp.com` tab, focuses that signed-in session, optionally searches for the named contact, verifies the visible chat header, types the exact draft, and verifies the outgoing message after confirmation.

## Installation

1. Extract the complete package.
2. Run `KB_Jarvis_OS.exe`.
3. Open `chrome://extensions/`.
4. Remove or reload the old KB Jarvis Browser Companion.
5. Enable Developer mode.
6. Choose **Load unpacked** and select the included `BrowserCompanion` folder.
7. Confirm Browser Companion version `13.0.0`.
8. Open **Gemini & Voice Settings** in Jarvis and save the API key.
9. Use **START LISTENING** for voice.
10. Use **START SCREEN** or **START CAMERA** only when that visual context is needed.

## Acceptance commands

```text
Who created you?
```

```text
Search my files for "price list" and show the best matches.
```

```text
Use Windows search to find Calculator and open the top result.
```

With an existing signed-in WhatsApp tab:

```text
Find Zain in my existing WhatsApp tab and draft: "KB Jarvis 13 test"
```

```text
Send "Meeting is tomorrow at 12" to Zain on WhatsApp.
```

Jarvis must request one confirmation before sending.

For visual operation:

1. Enable screen vision.
2. Start voice or enter a typed instruction.
3. Ask Jarvis to inspect the current screen and perform one normal click/type/scroll action at a time.

## Operational boundaries

- API quota and billing are controlled by the configured Google project.
- Login, CAPTCHA and two-factor authentication cannot be bypassed.
- Windows can block injected input into UAC secure desktop or higher-integrity windows.
- Camera access remains subject to Windows privacy permissions.
- Visual actions depend on the accuracy and freshness of the enabled visual frame.
- Message sending, publishing, deletion, purchases and Windows power actions remain confirmation protected.
- Jarvis reports completion only after the local tool returns completed or verified status.
- `Close Jarvis` closes only Jarvis and never shuts down Windows.
