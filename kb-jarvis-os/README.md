# KB Jarvis OS 14

**Developer:** KB (Khuda Bakhsh)  
**Product type:** Native Windows multimodal work, browser, file and website assistant  
**Release standard:** A package is released only after JavaScript validation, .NET compilation, self-contained publication and executable verification succeed.

## Version 14 priorities

Version 14 focuses on reliable real work rather than decorative automation:

- bounded, priority-based Gemini Live audio streaming
- stale microphone and visual packets are dropped instead of accumulating
- low-latency screen and camera previews
- verified folder creation
- safe multi-location file organization
- move or copy with collision-safe filenames
- custom HTML, CSS, JavaScript and PHP Website Studio
- timestamped website-file backups before edits
- page creation, exact file writing, search-and-replace and basic validation
- verified YouTube search and playback workflow
- existing-session WordPress page and post workflow
- optional content transfer from the latest readable ChatGPT response
- existing-session-only WhatsApp messaging
- responsive high-speed command-centre interface
- permanent creator identity: KB (Khuda Bakhsh)

## Why voice could glitch in Version 13

Version 13 could start a separate network send operation for each microphone callback while visual frames and tool responses used the same Live WebSocket. On slower computers or unstable networks, pending sends could accumulate and produce delayed, broken or robotic speech.

Version 14 routes microphone packets and visual frames through bounded queues:

- microphone audio receives priority;
- only a short recent audio window is retained;
- only the latest visual frame is retained;
- excessive playback backlog is cleared;
- screen and camera preview updates are independent of Live transmission.

Hardware, microphone drivers, Bluetooth audio, CPU load and internet quality can still affect voice quality, but the application no longer creates an unlimited media-send backlog.

## File Organizer

Jarvis can create a destination folder and organize matching files from:

- Desktop
- Documents
- Downloads
- the combined safe `PC` scope, meaning the three user locations above
- another explicit accessible folder path

System folders, Windows directories and protected locations are not included in the automatic PC scope.

Example:

```text
Jarvis, meray PC par Notepad wali sab .txt files ko Desktop par
"Organized Notepad Files" folder mein move kar do.
```

Expected workflow:

1. Scan the selected user locations.
2. Show the number and preview of matching files.
3. Request one confirmation before moving or copying.
4. Create the destination folder.
5. Use collision-safe names such as `notes (2).txt` when needed.
6. Verify each destination file.
7. Open the completed folder.

Folder-only command:

```text
Create a folder named Website Work on my Desktop.
```

## YouTube playback

Example:

```text
Open YouTube and play Afreen Afreen Coke Studio.
```

Jarvis searches YouTube, opens a normal video result, attempts playback and checks the HTML video playback state. Browser autoplay policy, ads, age restrictions or account prompts can still block immediate playback; Jarvis must report that state honestly.

## Website Studio

By default, named projects are created inside:

```text
Documents\KB Jarvis Websites\<Project Name>
```

Supported operations:

- create a responsive HTML/CSS/JavaScript project
- create additional HTML pages
- create or update `.html`, `.css`, `.js`, `.php`, `.json` and other text files
- exact search-and-replace
- list project files
- open the project folder
- basic HTML structure validation
- basic HTML/CSS/JavaScript/PHP file counts
- timestamped backup before every existing-file change

Examples:

```text
Create a custom HTML CSS JavaScript website project named Fashion Studio.
```

```text
In Fashion Studio create a page pages/about.html titled About the Designer.
```

```text
Update css/style.css in Fashion Studio with this complete CSS: ...
```

```text
In Fashion Studio index.html replace "Old Heading" with "New Heading".
```

```text
Validate the Fashion Studio website project.
```

For large coding tasks, Jarvis can generate the requested code through Gemini and then call the verified Website Studio file tool. Existing files receive a timestamped `.kb-backup-*` copy before replacement.

## WordPress work

Precondition: a signed-in WordPress `wp-admin` tab must already exist in Chrome.

Supported operations:

- create a post
- create a page
- update the currently open editor
- save as draft
- publish after one final confirmation
- use supplied content
- use the latest readable assistant response from an existing ChatGPT tab

Examples:

```text
Create a WordPress draft post titled "Summer Fashion Guide"
and write the complete article yourself.
```

```text
Use the latest response in my existing ChatGPT tab and create a WordPress
draft page titled "About Our Studio".
```

```text
Publish the prepared WordPress post.
```

Publishing must request one final confirmation. WordPress plugins, custom editors, Elementor versions and site-specific admin layouts can differ. Version 14 directly supports common Gutenberg and classic editor controls, while unusual editors may require screen vision and one-step visual operation.

## WhatsApp rule

For messaging, Jarvis does not create, refresh or navigate to a new WhatsApp Web tab. It uses an existing signed-in tab, optionally searches for the contact, verifies the chat header, types the exact draft and verifies the outgoing message after confirmation.

## Installation

1. Extract the complete Version 14 package.
2. Run `KB_Jarvis_OS.exe`.
3. Open `chrome://extensions/`.
4. Remove or reload the older KB Jarvis Browser Companion.
5. Enable Developer mode.
6. Select **Load unpacked** and choose the included `BrowserCompanion` folder.
7. Confirm Browser Companion version `14.0.0`.
8. Open **Gemini & Voice Settings** and save the API key.
9. Start voice only when needed.
10. Enable screen or camera vision only when visual context is useful.

## First acceptance commands

```text
Who created you?
```

```text
Organize all Notepad text files in my safe PC user folders into a Desktop
folder named Organized Notepad Files.
```

```text
Open YouTube and play Pasoori Coke Studio.
```

```text
Create a custom HTML CSS JavaScript website project named Tomorrow Website.
```

```text
Create a WordPress draft post titled "Jarvis Test" with content
"Version 14 WordPress draft is working."
```

With an existing signed-in WhatsApp tab:

```text
Send "Version 14 is ready" to Zain on WhatsApp.
```

## Operational boundaries

- Login, CAPTCHA and two-factor authentication cannot be bypassed.
- WordPress, YouTube and WhatsApp depend on an accessible signed-in browser session where required.
- UAC secure desktop and higher-integrity windows can block normal injected input.
- Camera access depends on Windows privacy permission and the camera driver.
- Bluetooth devices and overloaded audio drivers can still cause hardware-level voice interruptions.
- Moving files, sending messages, publishing, deleting, purchases and Windows power operations remain confirmation protected.
- Jarvis reports completion only when the local skill or browser companion returns supporting evidence.
- `Close Jarvis` closes only Jarvis and never shuts down Windows.