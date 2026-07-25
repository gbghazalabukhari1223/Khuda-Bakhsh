# Privacy and Data Flow

## Local data

KB Jarvis OS stores configuration, logs and workspace data under the current Windows user's local application-data folder:

```text
%LOCALAPPDATA%\KB Jarvis OS\
```

The Gemini API key is encrypted for the current Windows account using Windows Data Protection API (DPAPI). It is not intentionally written to the application log.

## Microphone, screen and camera

Microphone audio is captured only after the user starts listening. Screen and camera frames are captured only after their separate on/off controls are enabled. Enabled audio or visual data may be transmitted to the configured Gemini API service to provide live understanding and responses.

Turn listening, screen or camera off when that context is not needed. Do not display passwords, financial information, confidential documents or private third-party material while screen sharing is enabled.

## Browser Companion

The Browser Companion can access supported browser tabs required for authorised tasks, including WhatsApp Web, YouTube, ChatGPT and WordPress administration pages. It communicates with the local Jarvis bridge on `127.0.0.1` and is designed to reuse existing signed-in sessions.

It does not bypass authentication, CAPTCHA or two-factor verification. Browser permissions are visible in the extension manifest and in Chrome or Edge extension settings.

## Logs

Operational logs may contain task descriptions, application states, errors and verification evidence. They are stored locally unless the user deliberately shares them. Review logs before sending them to another person because a task description may contain private text.

## Third-party services

Data sent to Gemini, WhatsApp, YouTube, WordPress, ChatGPT or other websites is governed by the relevant provider's terms and privacy practices. KB Jarvis OS cannot control how those providers retain or process data.

## User control

The authorised user controls:

- whether voice listening is active;
- whether screen context is active;
- whether camera context is active;
- whether consequential actions such as sending, publishing or moving files are confirmed;
- whether the Browser Companion is installed or enabled;
- deletion of local settings, logs and workspace data.
