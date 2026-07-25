# KB Jarvis OS 15 Acceptance Tests

## 1. Native startup and identity

Run `KB_Jarvis_OS.exe` and resize the window from wide to compact.

Expected:

- native WPF window opens;
- visible branding changes to Version 15;
- `FEMALE EXECUTIVE ONLINE` is shown;
- command area remains usable in compact mode;
- `Who created you?` returns KB (Khuda Bakhsh) and the professional female assistant identity.

## 2. Female voice and fluent audio

1. Open Gemini & Voice Settings.
2. Select `Laomedeia`.
3. Start listening.
4. Speak naturally for at least 45 seconds.
5. Allow Jarvis to speak several multi-sentence responses.
6. Enable screen vision during the same session.

Expected:

- CONNECTING or RESUMING → CONNECTED → LISTENING;
- a professional female voice is heard;
- microphone packets remain bounded;
- the microphone is suppressed while streamed Jarvis speech is actively playing, reducing self-echo;
- audio has priority over visual frames;
- stale speaker backlog is cleared instead of playing several seconds late;
- screen vision does not create an unlimited audio or video queue.

Repeat once using built-in audio and once using Bluetooth. Driver or network problems must be reported honestly rather than marked as a verified application success.

## 3. Temporary Live reconnect

During an active conversation, cause one temporary network interruption and restore it.

Expected:

- Jarvis reports reconnecting;
- saved session-resumption handle is reused when available;
- the Live session resumes without forcing a full manual restart;
- typed tools remain available while voice reconnects.

## 4. Verified multi-task mission

Command:

```text
Open Calculator, then play Pasoori on YouTube,
then create a Desktop folder named Jarvis Batch Test,
and finally list my active browser tabs.
```

Expected:

- Gemini calls the sequential task-batch tool;
- no more than one batch runs at a time;
- tasks remain in the supplied order;
- each task starts only after the previous result is returned;
- mission timeline shows `QUEUE 1/4`, `QUEUE 2/4`, and so on;
- a combined verified report is produced.

## 5. Multi-task failure policy

Give a batch containing one impossible middle task.

Expected:

- default behaviour stops after the failed task;
- later tasks do not silently run unless `continue_on_failure` was explicitly selected by the planner;
- the exact failed task and blocker are shown.

## 6. Batch confirmation safety

Command:

```text
Create a Desktop folder named Organised Notes,
move all PC Notepad files into it,
then open Calculator.
```

Expected:

- folder preparation can occur safely;
- file movement pauses for one confirmation;
- later tasks do not run while confirmation is pending;
- no files move before approval;
- the queue reports the paused task accurately.

## 7. Create and organise files

Place harmless `.txt` files in Desktop, Documents and Downloads, then say:

```text
Meray PC par Notepad wali sab files ko Desktop ke
Organized Notepad Files folder mein move kar do.
```

Expected:

- safe PC scope includes Desktop, Documents and Downloads;
- protected Windows and Program Files locations are excluded;
- `*.txt` is inferred;
- matching filenames are previewed;
- one confirmation is requested;
- collision-safe names are used;
- each destination file is verified;
- the result folder opens.

## 8. WhatsApp existing-session send

Precondition: an existing signed-in WhatsApp Web tab.

```text
Send "KB Jarvis Version 15 test" to Zain on WhatsApp.
```

Expected:

- no duplicate WhatsApp tab is created;
- contact search and chat-header verification run;
- exact draft is verified;
- one confirmation is requested;
- outgoing message is found before success is reported.

## 9. YouTube playback

```text
Open YouTube and play Afreen Afreen Coke Studio.
```

Expected:

- YouTube tab is reused or created;
- search results page opens;
- a normal video result is selected;
- playback is attempted;
- `playing=true` is reported only when the HTML video is actually playing;
- ads, autoplay blocks or account prompts are reported.

## 10. Screen and camera

- Start screen vision and move a window.
- Start camera vision and move a visible object.

Expected:

- previews update under normal load without a growing frame delay;
- only the latest compressed visual frame enters the Live stream;
- missing camera permission gives a precise blocker;
- visual desktop actions follow observe → one action → updated observation.

## 11. Website project

```text
Create a responsive HTML CSS JavaScript website project named Tomorrow Website and open it.
```

Expected project:

```text
Documents\KB Jarvis Websites\Tomorrow Website
```

Expected verified items:

- `index.html`;
- `css/style.css`;
- `js/app.js`;
- `images` folder;
- `pages` folder.

## 12. Website page and code editing

```text
In Tomorrow Website create pages/about.html titled About Us.
```

Then:

```text
Update css/style.css in Tomorrow Website with this complete CSS:
body { font-family: Arial; }
```

Expected:

- page and asset links are valid for the project layout;
- existing CSS receives a timestamped backup;
- replacement uses a temporary file;
- exact disk content is read back and verified.

## 13. WordPress draft

Precondition: existing signed-in WordPress `/wp-admin/` tab.

```text
Create a WordPress draft post titled "Jarvis Test"
with content "Version 15 female executive test".
```

Expected:

- existing admin session is used;
- common Gutenberg or classic controls are filled;
- draft is saved;
- no publish confirmation is requested for draft-only work.

## 14. WordPress from ChatGPT

Preconditions: existing ChatGPT tab with a readable latest assistant response and signed-in WordPress admin tab.

```text
Use the latest response in my ChatGPT tab and create a WordPress draft page titled About Our Studio.
```

Expected:

- latest assistant response is read through Browser Companion;
- content is inserted directly into WordPress;
- no false clipboard claim is made;
- draft state is reported.

## 15. Consequential actions and honesty

- message sending requires one confirmation;
- publishing requires one confirmation;
- moving files requires one confirmation;
- passwords, CAPTCHA, two-factor authentication and UAC secure desktop are not bypassed;
- unsupported work returns the exact blocker;
- `Close Jarvis` closes only Jarvis and never shuts down Windows.
