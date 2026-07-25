# Changelog

## 15.0.0 — Female Executive Release

### Added

- Professional female multilingual persona addressing the primary user as Boss.
- Gemini Live voice with bounded microphone buffering, audio priority, echo suppression, context compression and session resumption.
- Ordered multi-task mission queue with per-task verification and safe confirmation pauses.
- File and folder creation, safe Desktop/Documents/Downloads search, move/copy organisation and collision-safe naming.
- Custom Website Studio for HTML, CSS, JavaScript, PHP and other text/code files with automatic backups.
- Existing-session WhatsApp drafting and confirmed sending.
- YouTube search, result selection and playback-state verification.
- Existing-session WordPress draft/page/post workflows.
- Screen and camera context with explicit on/off controls.
- Native mouse, keyboard, taskbar search and browser-tab tools.

### Fixed

- Corrected Version 15 startup to initialise Version 14 work skills before applying Version 15 persona and mission-queue features.
- Removed all post-build binary patching. Release executables are produced only from clean source compilation.
- Reduced voice glitches caused by unbounded media sends and speaker-to-microphone echo.
- Prevented WhatsApp commands from deliberately opening duplicate WhatsApp Web tabs.

### Known boundaries

- Passwords, CAPTCHA, two-factor authentication and UAC secure desktop are not bypassed.
- Highly customised WordPress/Elementor layouts may require visual one-step operation.
- Voice quality can still be affected by Bluetooth drivers, Windows audio enhancements, CPU load, network latency and Gemini service availability.
