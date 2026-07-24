# KB Jarvis OS v11 Acceptance Tests

## Identity

Command: `Who created you?`

Expected: Jarvis identifies KB (Khuda Bakhsh) as its creator and developer.

## Notepad

Command: `Open Notepad and write: "KB Jarvis OS native Notepad skill is working."`

Expected:

1. A text file is created on Desktop.
2. File content exactly matches the requested text.
3. Notepad opens the verified file.
4. Jarvis reports the verified path.

## Browser Companion

Expected:

1. Native app listens on one port from 32145 to 32155.
2. Version 11 extension reconnects automatically.
3. Dashboard shows `CONNECTED`.
4. Closing and reopening Chrome does not require a new pair code.

## WhatsApp current chat draft

Precondition: WhatsApp Web is logged in and a chat is already open.

Command: `In the current WhatsApp chat, draft: "KB Jarvis OS test message"`

Expected:

1. Existing WhatsApp tab is selected.
2. Current chat header is inspected.
3. Composer receives focus.
4. Exact text appears in the composer.
5. Text is not sent.
6. Jarvis reports draft verification.

## WhatsApp current chat send

Command: `In the current WhatsApp chat, send: "KB Jarvis OS verified send test"`

Expected:

1. Jarvis requests one confirmation.
2. Command `Haan, bhej do` resumes the pending mission.
3. Message is sent.
4. Outgoing message bubble is found.
5. Jarvis reports verified completion.

## Lifecycle safety

Command: `Close Jarvis`

Expected: Only KB Jarvis closes. Windows does not shut down, restart, sleep or lock.
