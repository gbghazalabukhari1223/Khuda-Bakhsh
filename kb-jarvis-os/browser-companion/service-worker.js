const PORTS = Array.from({ length: 11 }, (_, index) => 32145 + index);
const RECONNECT_DELAY_MS = 1800;
let socket = null;
let activePort = null;
let connecting = false;

chrome.runtime.onInstalled.addListener(() => {
  chrome.alarms.create('kb-jarvis-heartbeat', { periodInMinutes: 0.5 });
  connectLoop();
});

chrome.runtime.onStartup.addListener(connectLoop);
chrome.alarms.onAlarm.addListener((alarm) => {
  if (alarm.name === 'kb-jarvis-heartbeat' && (!socket || socket.readyState !== WebSocket.OPEN)) {
    connectLoop();
  }
});

chrome.runtime.onMessage.addListener((message, _sender, sendResponse) => {
  if (message?.type === 'status') {
    sendResponse({ connected: socket?.readyState === WebSocket.OPEN, port: activePort, version: '11.0.0' });
    return true;
  }
  if (message?.type === 'reconnect') {
    connectLoop(true);
    sendResponse({ accepted: true });
    return true;
  }
  return false;
});

async function connectLoop(force = false) {
  if (connecting) return;
  if (!force && socket?.readyState === WebSocket.OPEN) return;
  connecting = true;

  try {
    if (socket) {
      try { socket.close(); } catch { /* no-op */ }
      socket = null;
    }

    for (const port of PORTS) {
      const candidate = await connectToPort(port);
      if (candidate) {
        socket = candidate;
        activePort = port;
        await chrome.storage.local.set({ activePort: port, lastConnectedAt: Date.now() });
        return;
      }
    }
  } finally {
    connecting = false;
  }

  setTimeout(connectLoop, RECONNECT_DELAY_MS);
}

function connectToPort(port) {
  return new Promise((resolve) => {
    let settled = false;
    const candidate = new WebSocket(`ws://127.0.0.1:${port}/ws`);
    const timeout = setTimeout(() => finish(null), 650);

    const finish = (value) => {
      if (settled) return;
      settled = true;
      clearTimeout(timeout);
      resolve(value);
    };

    candidate.onopen = () => {
      candidate.onmessage = (event) => handleCommandMessage(event.data, candidate);
      candidate.onclose = () => {
        if (socket === candidate) {
          socket = null;
          activePort = null;
          setTimeout(connectLoop, RECONNECT_DELAY_MS);
        }
      };
      candidate.onerror = () => { /* onclose handles retry */ };
      candidate.send(JSON.stringify({
        type: 'hello',
        product: 'KB Jarvis OS Browser Companion',
        version: '11.0.0',
        developer: 'KB (Khuda Bakhsh)'
      }));
      finish(candidate);
    };

    candidate.onerror = () => finish(null);
    candidate.onclose = () => finish(null);
  });
}

async function handleCommandMessage(raw, channel) {
  let command;
  try {
    command = JSON.parse(raw);
  } catch {
    return;
  }

  if (command?.type !== 'command' || !command.id || !command.operation) return;

  try {
    const data = await executeOperation(command.operation, command.payload ?? {});
    sendResult(channel, command.id, true, data, null);
  } catch (error) {
    sendResult(channel, command.id, false, null, error instanceof Error ? error.message : String(error));
  }
}

function sendResult(channel, id, success, data, error) {
  if (channel.readyState !== WebSocket.OPEN) return;
  channel.send(JSON.stringify({ type: 'result', id, success, data, error }));
}

async function executeOperation(operation, payload) {
  switch (operation) {
    case 'tabs.list':
      return await listTabs();
    case 'whatsapp.current_chat.inspect':
      return await inspectCurrentWhatsAppChat();
    case 'whatsapp.current_chat.draft':
      return await draftInCurrentWhatsAppChat(String(payload.message ?? ''));
    case 'whatsapp.current_chat.send':
      return await sendInCurrentWhatsAppChat(String(payload.message ?? ''));
    default:
      throw new Error(`Unsupported Browser Companion operation: ${operation}`);
  }
}

async function listTabs() {
  const tabs = await chrome.tabs.query({});
  return tabs.map((tab) => ({ id: tab.id, title: tab.title, url: tab.url, active: tab.active, windowId: tab.windowId }));
}

async function findWhatsAppTab() {
  const tabs = await chrome.tabs.query({ url: ['https://web.whatsapp.com/*'] });
  if (!tabs.length) throw new Error('No existing WhatsApp Web tab was found. Open WhatsApp Web and sign in once.');
  const active = tabs.find((tab) => tab.active) ?? tabs[0];
  await chrome.tabs.update(active.id, { active: true });
  await chrome.windows.update(active.windowId, { focused: true });
  return active;
}

async function inspectCurrentWhatsAppChat() {
  const tab = await findWhatsAppTab();
  const [result] = await chrome.scripting.executeScript({
    target: { tabId: tab.id },
    world: 'MAIN',
    func: inspectWhatsAppPage
  });
  if (!result?.result?.ok) throw new Error(result?.result?.error ?? 'WhatsApp current-chat inspection failed.');
  return { tabId: tab.id, ...result.result };
}

async function draftInCurrentWhatsAppChat(message) {
  if (!message.trim()) throw new Error('The WhatsApp draft message is empty.');
  const tab = await findWhatsAppTab();
  const state = await focusWhatsAppComposer(tab.id);
  await debuggerType(tab.id, message, false);
  const verified = await verifyComposerText(tab.id, message);
  if (!verified.ok) throw new Error(`Draft verification failed. Composer contained: ${verified.actual ?? '<empty>'}`);
  return { tabId: tab.id, chatHeader: state.chatHeader, draftVerified: true, message };
}

async function sendInCurrentWhatsAppChat(message) {
  if (!message.trim()) throw new Error('The WhatsApp message is empty.');
  const tab = await findWhatsAppTab();
  const state = await focusWhatsAppComposer(tab.id);
  await debuggerType(tab.id, message, true);
  const sent = await verifyOutgoingMessage(tab.id, message);
  if (!sent.ok) throw new Error('The Enter key was sent, but the outgoing message could not be verified in the current chat.');
  return { tabId: tab.id, chatHeader: state.chatHeader, sentVerified: true, message };
}

async function focusWhatsAppComposer(tabId) {
  const [result] = await chrome.scripting.executeScript({
    target: { tabId },
    world: 'MAIN',
    func: focusCurrentComposer
  });
  if (!result?.result?.ok) throw new Error(result?.result?.error ?? 'WhatsApp message composer was not found.');
  return result.result;
}

async function verifyComposerText(tabId, expected) {
  const [result] = await chrome.scripting.executeScript({
    target: { tabId },
    world: 'MAIN',
    args: [expected],
    func: (text) => {
      const composer = findVisibleComposer();
      if (!composer) return { ok: false, actual: null };
      const actual = (composer.innerText || composer.textContent || '').replace(/\u00a0/g, ' ').trim();
      return { ok: actual === text.trim(), actual };
    }
  });
  return result?.result ?? { ok: false, actual: null };
}

async function verifyOutgoingMessage(tabId, expected) {
  const deadline = Date.now() + 6500;
  while (Date.now() < deadline) {
    const [result] = await chrome.scripting.executeScript({
      target: { tabId },
      world: 'MAIN',
      args: [expected],
      func: (text) => {
        const outgoing = [...document.querySelectorAll('[data-testid="msg-container"], .message-out')]
          .filter((node) => isVisible(node));
        const normalized = text.trim();
        return { ok: outgoing.some((node) => (node.innerText || '').includes(normalized)) };
      }
    });
    if (result?.result?.ok) return { ok: true };
    await delay(350);
  }
  return { ok: false };
}

async function debuggerType(tabId, text, pressEnter) {
  const target = { tabId };
  let attached = false;
  try {
    await chrome.debugger.attach(target, '1.3');
    attached = true;
    await chrome.debugger.sendCommand(target, 'Input.dispatchKeyEvent', {
      type: 'keyDown', key: 'a', code: 'KeyA', windowsVirtualKeyCode: 65, modifiers: 2
    });
    await chrome.debugger.sendCommand(target, 'Input.dispatchKeyEvent', {
      type: 'keyUp', key: 'a', code: 'KeyA', windowsVirtualKeyCode: 65, modifiers: 2
    });
    await chrome.debugger.sendCommand(target, 'Input.dispatchKeyEvent', {
      type: 'keyDown', key: 'Backspace', code: 'Backspace', windowsVirtualKeyCode: 8
    });
    await chrome.debugger.sendCommand(target, 'Input.dispatchKeyEvent', {
      type: 'keyUp', key: 'Backspace', code: 'Backspace', windowsVirtualKeyCode: 8
    });
    await chrome.debugger.sendCommand(target, 'Input.insertText', { text });
    if (pressEnter) {
      await delay(120);
      await chrome.debugger.sendCommand(target, 'Input.dispatchKeyEvent', {
        type: 'keyDown', key: 'Enter', code: 'Enter', windowsVirtualKeyCode: 13
      });
      await chrome.debugger.sendCommand(target, 'Input.dispatchKeyEvent', {
        type: 'keyUp', key: 'Enter', code: 'Enter', windowsVirtualKeyCode: 13
      });
    }
  } finally {
    if (attached) {
      try { await chrome.debugger.detach(target); } catch { /* no-op */ }
    }
  }
}

function inspectWhatsAppPage() {
  const qr = document.querySelector('canvas[aria-label*="QR" i], [data-ref] canvas');
  if (qr && isVisible(qr)) return { ok: false, error: 'WhatsApp Web login QR is visible.' };
  const composer = findVisibleComposer();
  const chatHeader = readChatHeader();
  if (!composer) return { ok: false, error: 'No visible current-chat message composer was found.' };
  return { ok: true, chatHeader, composerFound: true };
}

function focusCurrentComposer() {
  const qr = document.querySelector('canvas[aria-label*="QR" i], [data-ref] canvas');
  if (qr && isVisible(qr)) return { ok: false, error: 'WhatsApp Web login QR is visible.' };
  const composer = findVisibleComposer();
  if (!composer) return { ok: false, error: 'No visible message composer was found in the current WhatsApp chat.' };
  composer.scrollIntoView({ block: 'center', inline: 'nearest' });
  composer.click();
  composer.focus();
  const active = document.activeElement;
  const focused = active === composer || composer.contains(active);
  return { ok: focused, chatHeader: readChatHeader(), tag: composer.tagName, role: composer.getAttribute('role') };
}

function findVisibleComposer() {
  const selectors = [
    'footer [contenteditable="true"][role="textbox"]',
    'footer div[contenteditable="true"]',
    '[data-testid="conversation-compose-box-input"]',
    'div[contenteditable="true"][aria-placeholder*="message" i]',
    'div[contenteditable="true"][aria-label*="message" i]'
  ];
  const candidates = selectors.flatMap((selector) => [...document.querySelectorAll(selector)]);
  const unique = [...new Set(candidates)].filter(isVisible);
  return unique
    .filter((node) => node.getBoundingClientRect().top > window.innerHeight * 0.55)
    .sort((a, b) => b.getBoundingClientRect().top - a.getBoundingClientRect().top)[0] ?? null;
}

function readChatHeader() {
  const selectors = [
    'header span[title]',
    'header [data-testid="conversation-info-header-chat-title"]',
    'header [dir="auto"]'
  ];
  for (const selector of selectors) {
    const node = [...document.querySelectorAll(selector)].find(isVisible);
    const text = node?.getAttribute('title') || node?.innerText || node?.textContent;
    if (text?.trim()) return text.trim();
  }
  return null;
}

function isVisible(node) {
  if (!(node instanceof Element)) return false;
  const rect = node.getBoundingClientRect();
  const style = getComputedStyle(node);
  return rect.width > 2 && rect.height > 2 && style.visibility !== 'hidden' && style.display !== 'none' && Number(style.opacity || 1) > 0;
}

function delay(milliseconds) {
  return new Promise((resolve) => setTimeout(resolve, milliseconds));
}

connectLoop();
