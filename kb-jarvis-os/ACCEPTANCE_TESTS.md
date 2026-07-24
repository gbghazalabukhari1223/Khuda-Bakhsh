# KB Jarvis OS 12 Acceptance Tests

## 1. Native startup

Expected:

1. `KB_Jarvis_OS.exe` opens its own WPF window, not an Edge app window.
2. The dashboard shows Version 12.0.0 and Developer KB (Khuda Bakhsh).
3. The local Browser Companion bridge starts on a port from 32145 to 32155.

## 2. Secure settings

1. Open **Gemini & Voice Settings**.
2. Save a valid API key, Live model, text model and voice.
3. Close and reopen Jarvis.

Expected: settings remain available for the same Windows account and the API key is stored through Windows DPAPI rather than plaintext.

## 3. Typed intelligence

Command: `Hello Jarvis. Who created you?`

Expected:

1. Gemini processes the natural-language request.
2. Jarvis identifies KB (Khuda Bakhsh) as creator and developer.
3. A text response appears in the mission panel and timeline.
4. When spoken fallback is enabled, Windows also reads the reply aloud.

## 4. Gemini tool calling

Command: `Open Calculator.`

Expected:

1. Gemini selects the local application tool.
2. The deterministic App Launch skill runs.
3. Calculator opens.
4. The tool result is returned to Gemini.
5. Jarvis reports the local result rather than merely saying it can help.

## 5. Live voice

1. Click **START LISTENING**.
2. Say: `Jarvis, who created you?`
3. Say: `Open Calculator.`

Expected:

1. Voice state changes through CONNECTING, CONNECTED and LISTENING.
2. Microphone input transcription appears in the timeline.
3. Gemini audio is played through the selected output device.
4. The application command is executed through the local skill engine.
5. A temporary Live disconnect triggers reconnect when enabled.

## 6. Notepad

Command: `Open Notepad and write: "KB Jarvis OS 12 native Notepad skill is working."`

Expected:

1. A text file is created on Desktop.
2. File content exactly matches the requested text.
3. Notepad opens the verified file.
4. Jarvis reports the verified path.

## 7. Browser Companion

Expected:

1. Browser Companion version is 12.0.0.
2. It probes the silent health endpoint before opening a WebSocket.
3. It reconnects automatically without recurring pair codes.
4. Dashboard shows CONNECTED when the command channel is alive.
5. Closing Jarvis shows an offline state without filling Chrome with refused-WebSocket errors.

## 8. WhatsApp current chat draft

Precondition: WhatsApp Web is logged in and a chat is already open.

Command: `In the current WhatsApp chat, draft: "KB Jarvis OS 12 test message"`

Expected:

1. Existing WhatsApp tab is selected.
2. Current chat header is inspected.
3. Composer receives focus.
4. Exact text appears in the composer.
5. Text is not sent.
6. Jarvis reports draft verification.

## 9. WhatsApp current chat send

Command: `In the current WhatsApp chat, send: "KB Jarvis OS 12 verified send test"`

Expected:

1. Jarvis requests one confirmation.
2. Spoken or typed `Haan, bhej do` resumes the pending mission.
3. Message is sent.
4. Outgoing message bubble is found.
5. Jarvis reports verified completion.

## 10. Lifecycle safety

Command: `Close Jarvis`

Expected: Only KB Jarvis closes. Windows does not shut down, restart, sleep or lock.

## 11. Honest failure reporting

Request an unsupported workflow such as independently finding an ambiguous WhatsApp contact and forwarding media.

Expected: Jarvis must not report completion. It should identify the missing verified skill or precise blocker rather than pretending the task succeeded.
