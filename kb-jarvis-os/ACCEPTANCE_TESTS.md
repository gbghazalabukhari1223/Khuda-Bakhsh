# KB Jarvis OS 14 Acceptance Tests

## 1. Native startup and responsive command centre

1. Run `KB_Jarvis_OS.exe`.
2. Resize from wide to compact.

Expected:

- Native WPF window opens.
- Version 14.0.0 and Developer KB (Khuda Bakhsh) are visible.
- Wide layout shows navigation, work core and telemetry.
- Compact layout keeps the mission command box usable.
- Work-core indicators show audio queue, file organizer, Website Studio and browser agent.

## 2. Voice stability

1. Configure Gemini.
2. Click **START LISTENING**.
3. Speak continuously for at least 30 seconds.
4. Enable screen vision during the conversation.
5. Ask Jarvis to open Calculator.

Expected:

- CONNECTING → CONNECTED → LISTENING.
- Speech is transcribed and answered.
- Calculator opens through the local tool.
- Microphone packets do not build an unlimited async-send backlog.
- Only a bounded recent audio window and latest visual frame are retained.
- Voice remains responsive when screen vision is active.

Also test with Bluetooth and built-in speakers separately. Hardware or driver interruptions must be reported as environmental rather than falsely marked as an application success.

## 3. Screen and camera delay

1. Start screen vision.
2. Move or resize a window.
3. Confirm the preview updates within approximately one second under normal load.
4. Start camera vision and move a visible object.

Expected:

- Previews refresh up to approximately twice per second.
- Only the latest compressed visual frame enters Gemini Live.
- Old screen or camera frames do not queue indefinitely.
- Missing camera permission produces a clear blocker.

## 4. Create a folder

Command:

```text
Create a folder named Website Work on my Desktop.
```

Expected:

- Folder is created.
- `Directory.Exists` verification succeeds.
- Exact path is reported.

## 5. Organize Desktop Notepad files

Create three harmless `.txt` test files on Desktop, then say:

```text
Organize all Notepad text files on my Desktop into a folder named Organized Notepad Files.
```

Expected:

1. Jarvis scans Desktop for `*.txt`.
2. It previews the matching count and filenames.
3. It asks one confirmation before moving.
4. After `Haan, kar do`, it creates the folder.
5. Files are moved.
6. Destination files are verified.
7. Original paths no longer exist.
8. Destination folder opens.

## 6. Safe PC user-folder organization

Place test `.txt` files in Desktop, Documents and Downloads, then say:

```text
Meray PC par Notepad wali sab files ko Desktop ke Organized Notes folder mein move kar do.
```

Expected:

- The safe PC scope includes Desktop, Documents and Downloads.
- Windows, Program Files and other protected system locations are not scanned.
- Duplicate filenames receive collision-safe names such as `notes (2).txt`.
- One confirmation is requested before movement.
- Every completed destination is verified.

## 7. File copy mode

```text
Copy all PDF files from Downloads into a folder named PDF Archive on Desktop.
```

Expected: source files remain, verified copies exist, collisions are handled and one confirmation is requested.

## 8. YouTube playback

```text
Open YouTube and play Pasoori Coke Studio.
```

Expected:

- Browser Companion opens or reuses YouTube.
- Search query is entered through a YouTube results URL.
- A normal video result is selected.
- Video page opens.
- Playback is attempted.
- Jarvis reports verified playing only when the HTML video is not paused and not ended.
- Ads, autoplay blocking or account prompts are reported honestly.

## 9. Website project creation

```text
Create a custom HTML CSS JavaScript website project named Tomorrow Website and open it.
```

Expected project location:

```text
Documents\KB Jarvis Websites\Tomorrow Website
```

Expected files/folders:

- `index.html`
- `css/style.css`
- `js/app.js`
- `images`
- `pages`

All required files are verified before completion is reported.

## 10. Website page creation

```text
In Tomorrow Website create pages/about.html titled About Us.
```

Expected:

- Page is created inside the project.
- CSS and JavaScript relative links point back to project assets.
- Exact file content is read back and verified.

## 11. Website edit and backup

First save known content in `css/style.css`, then say:

```text
Update css/style.css in Tomorrow Website with this complete CSS: body { font-family: Arial; }
```

Expected:

- Original file is copied to `style.css.kb-backup-<timestamp>`.
- New content is written through a temporary file.
- Temporary file atomically replaces the target.
- Exact disk content is verified.

## 12. Website search-and-replace

```text
In Tomorrow Website index.html replace "Professional custom website" with "Tomorrow's professional website".
```

Expected: occurrence count is reported, backup is created and replacement is verified.

## 13. Website validation

```text
Validate the Tomorrow Website project.
```

Expected: HTML, CSS, JavaScript and PHP file counts are reported; basic `<html>` and `<title>` checks run for HTML pages.

## 14. WordPress draft

Precondition: an existing signed-in Chrome tab is open under a WordPress `/wp-admin/` URL.

```text
Create a WordPress draft post titled "Jarvis Test" with content "Version 14 draft test".
```

Expected:

- Existing admin session is used.
- New post editor opens in that same signed-in tab.
- Common Gutenberg or classic title/content controls are filled.
- Draft-save control is clicked.
- Saved state is verified when visible.
- No publish confirmation is requested for draft-only work.

## 15. WordPress page from ChatGPT

Preconditions:

- Existing ChatGPT tab contains a readable latest assistant response.
- Existing WordPress admin tab is signed in.

Command:

```text
Use the latest response in my ChatGPT tab and create a WordPress draft page titled About Our Studio.
```

Expected:

- Latest `[data-message-author-role="assistant"]` response is read.
- Text is transferred directly through the Browser Companion, not through an unverified clipboard claim.
- WordPress page editor is filled.
- Draft is saved and verified where possible.

## 16. WordPress publish confirmation

```text
Publish the prepared WordPress post.
```

Expected: Jarvis requests one final confirmation. After confirmation, both the publish panel and final publish control are handled and published confirmation is checked.

## 17. Existing-session WhatsApp

Precondition: signed-in WhatsApp Web tab exists.

```text
Send "KB Jarvis 14 verified send" to Zain on WhatsApp.
```

Expected: no duplicate WhatsApp tab; contact and chat header are verified; one send confirmation; outgoing bubble verification.

## 18. Browser Companion 14

Expected:

- Extension version `14.0.0`.
- Automatic localhost reconnect.
- WhatsApp, YouTube, WordPress and ChatGPT-read permissions are present.
- JavaScript validation succeeds.
- Closing Jarvis changes the extension to offline without uncontrolled reconnection errors.

## 19. Lifecycle and safety

- `Close Jarvis` closes only Jarvis.
- Moving/copying files requires confirmation.
- Sending requires confirmation.
- WordPress publishing requires confirmation.
- Deletion, purchasing and power actions are not executed silently.
- Login, CAPTCHA, two-factor authentication and UAC secure desktop are not bypassed.

## 20. Honest result reporting

Request a workflow with an unsupported custom browser editor or inaccessible file.

Expected: Jarvis reports the exact unsupported selector, permission, authentication or verification blocker. An attempted click is not reported as completed work.