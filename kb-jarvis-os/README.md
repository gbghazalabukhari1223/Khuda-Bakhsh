# KB Jarvis OS v11

**Developer:** KB (Khuda Bakhsh)  
**Product type:** Native Windows AI operating environment  
**Status:** Active development

## Product goal

KB Jarvis OS is a native Windows desktop assistant that understands the Boss's goal, selects a trained local skill, executes the complete workflow, verifies the result, retries through alternate methods when necessary, and asks the Boss only for consequential approval, authentication, or genuinely missing information.

## Locked product identity

- Assistant: KB Jarvis
- Creator and developer: KB (Khuda Bakhsh)
- Primary user title: Boss
- Identity response: "Mujhe KB — Khuda Bakhsh ne design aur develop kiya hai. Main unka personal AI operating assistant hoon."

## V11 architecture

1. Native Windows host and system-tray worker
2. Animated futuristic command-centre UI
3. Planner–Skill–Executor–Verifier engine
4. Native desktop bridge for focus, mouse, keyboard, screenshots and UI Automation
5. Stable Browser Companion with persistent pairing and automatic reconnection
6. WhatsApp Executive Agent
7. Notepad and desktop-app skill pack
8. Teach Mode and versioned skill memory
9. Persistent encrypted memory
10. Auditable confirmation and safety layer

## First acceptance-critical skills

### WhatsApp current chat

- Detect existing WhatsApp tab
- Read current chat header
- Find and focus the composer
- Type and verify an exact draft
- Ask once before sending
- Send and verify the outgoing message

### WhatsApp contact search

- Parse single or multiple recipients
- Search candidates
- Rank and verify the selected chat header
- Draft, confirm, send and verify independently per recipient

### Notepad write and save

- Launch or find Notepad
- Activate the correct window
- Focus the editor
- Type with native Unicode input
- Verify visible text
- Save to the requested path
- Verify the file exists

## Development rule

A click, keystroke or tool call is not success. A task is complete only when the intended result has been verified.
