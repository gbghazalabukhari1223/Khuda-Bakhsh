# KB Jarvis OS 13 Acceptance Tests

## 1. Native startup and responsive interface

1. Run `KB_Jarvis_OS.exe`.
2. Resize the window from wide to compact.

Expected:

- Jarvis opens its own native WPF window.
- Version 13.0.0 and Developer KB (Khuda Bakhsh) are visible.
- Wide mode shows left navigation, core and right telemetry.
- Medium mode narrows side panels.
- Compact mode hides side panels while keeping the command area usable.

## 2. Live voice

1. Configure the Gemini API key.
2. Click **START LISTENING**.
3. Say: `Jarvis, who created you?`
4. Say: `Open Calculator.`

Expected: CONNECTING → CONNECTED → LISTENING; voice transcription appears; Jarvis speaks; Calculator opens through the local skill engine.

## 3. Screen vision

1. Click **START SCREEN**.
2. Confirm a live preview appears.
3. Enter: `Describe the application currently visible on my screen.`
4. Ask Jarvis to perform one normal click or typing action on a visible non-sensitive target.

Expected: screen state changes to LIVE, typed planning receives the screenshot, native input executes one action, and Jarvis observes an updated frame before continuing.

## 4. Camera vision

1. Click **START CAMERA**.
2. Allow Windows camera permission if requested.
3. Confirm a live preview appears.
4. Ask Jarvis to describe a visible object.

Expected: camera state changes to LIVE and the image is available to the AI. If no camera or permission is available, Jarvis reports that precise blocker.

## 5. Existing-session WhatsApp rule

Precondition: one signed-in `https://web.whatsapp.com` tab exists.

Command:

```text
Find Zain in my existing WhatsApp tab and draft: "KB Jarvis 13 contact test"
```

Expected:

- Jarvis does not create, navigate to, refresh or duplicate WhatsApp Web.
- The existing tab is focused.
- The WhatsApp search field is focused and cleared.
- Contact candidates are ranked.
- A candidate is opened and its chat header is verified.
- The exact draft is typed and verified but not sent.

## 6. WhatsApp verified send

```text
Send "KB Jarvis 13 verified send" to Zain on WhatsApp.
```

Expected: Jarvis verifies the contact/chat, requests one final confirmation, sends after `Haan, bhej do`, finds the outgoing message, and reports verified completion.

## 7. Missing WhatsApp tab

Close all WhatsApp Web tabs, then request a WhatsApp message.

Expected: Jarvis reports that no existing signed-in WhatsApp tab was found. It must not open a new logged-out WhatsApp page.

## 8. Local file search

```text
Search my files for "price list" and show the top matches.
```

Expected: Desktop, Documents, Downloads, Pictures, Music and Videos are searched with permission-safe traversal; ranked paths are shown; no false open result is reported.

Then:

```text
Find the latest matching price list and open the best match.
```

Expected: the top ranked result is opened through its default Windows application.

## 9. Windows taskbar search

```text
Use Windows search to find Calculator and open the top result.
```

Expected: Windows search opens, the query is typed through native Unicode input, and Enter is dispatched.

## 10. Notepad

```text
Open Notepad and write: "KB Jarvis OS 13 native Notepad skill is working."
```

Expected: a Desktop text file is created, exact content is verified, the file opens in Notepad and the path is reported.

## 11. Browser Companion

Expected:

- extension version 13.0.0;
- silent health probing before WebSocket connection;
- automatic reconnect without recurring pair codes;
- existing-tab listing works;
- app shutdown shows an offline state without repeated refused-WebSocket errors.

## 12. Lifecycle and consequential-action safety

- `Close Jarvis` closes only Jarvis.
- Message sending requires one confirmation.
- Deletion, publishing, purchasing and Windows power actions are not executed silently.
- CAPTCHA, passwords, two-factor authentication and UAC secure desktop are not bypassed.

## 13. Honest result reporting

Request an unsupported complex workflow.

Expected: Jarvis reports the exact unsupported step or authentication blocker. It never says `done` merely because a click or tool request was attempted.
