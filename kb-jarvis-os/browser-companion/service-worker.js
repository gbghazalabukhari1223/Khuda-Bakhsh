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
  if (alarm.name === 'kb-jarvis-heartbeat' && socket?.readyState !== WebSocket.OPEN) connectLoop();
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
  if (connecting || (!force && socket?.readyState === WebSocket.OPEN)) return;
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
    const timeout = setTimeout(() => finish(null), 700);
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
      candidate.onerror = () => { /* close event performs recovery */ };
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
  try { command = JSON.parse(raw); } catch { return; }
  if (command?.type !== 'command' || !command.id || !command.operation) return;
  try {
    const data = await executeOperation(command.operation, command.payload ?? {});
    sendResult(channel, command.id, true, data, null);
  } catch (error) {
    sendResult(channel, command.id, false, null, error instanceof Error ? error.message : String(error));
  }
}

function sendResult(channel, id, success, data, error) {
  if (channel.readyState === WebSocket.OPEN) {
    channel.send(JSON.stringify({ type: 'result', id, success, data, error }));
  }
}

async function executeOperation(operation, payload) {
  switch (operation) {
    case 'tabs.list': return await listTabs();
    case 'whatsapp.current_chat.inspect': return await inspectCurrentWhatsAppChat();
    case 'whatsapp.current_chat.draft': return await draftInCurrentWhatsAppChat(String(payload.message ?? ''));
    case 'whatsapp.current_chat.send': return await sendInCurrentWhatsAppChat(String(payload.message ?? ''));
    default: throw new Error(`Unsupported Browser Companion operation: ${operation}`);
  }
}

async function listTabs() {
  const tabs = await chrome.tabs.query({});
  return tabs.map((tab) => ({ id: tab.id, title: tab.title, url: tab.url, active: tab.active, windowId: tab.windowId }));
}

async function findWhatsAppTab() {
  const tabs = await chrome.tabs.query({ url: ['https://web.whatsapp.com/*'] });
  if (!tabs.length) throw new Error('No existing WhatsApp Web tab was found. Open WhatsApp Web and sign in once.');
  const selected = tabs.find((tab) => tab.active) ?? tabs[0];
  await chrome.tabs.update(selected.id, { active: true });
  await chrome.windows.update(selected.windowId, { focused: true });
  await ensurePageAgent(selected.id);
  return selected;
}

async function ensurePageAgent(tabId) {
  await chrome.scripting.executeScript({ target: { tabId }, world: 'MAIN', files: ['page-agent.js'] });
}

async function callPageAgent(tabId, method, args = []) {
  const [result] = await chrome.scripting.executeScript({
    target: { tabId },
    world: 'MAIN',
    args: [method, args],
    func: (methodName, methodArgs) => {
      const agent = window.__kbJarvisPageAgent;
      if (!agent || typeof agent[methodName] !== 'function') {
        return { ok: false, error: 'KB Jarvis page agent is not available.' };
      }
      return agent[methodName](...methodArgs);
    }
  });
  return result?.result ?? { ok: false, error: 'The page agent returned no result.' };
}

async function inspectCurrentWhatsAppChat() {
  const tab = await findWhatsAppTab();
  const state = await callPageAgent(tab.id, 'inspect');
  if (!state.ok) throw new Error(state.error ?? 'WhatsApp current-chat inspection failed.');
  return { tabId: tab.id, ...state };
}

async function draftInCurrentWhatsAppChat(message) {
  if (!message.trim()) throw new Error('The WhatsApp draft message is empty.');
  const tab = await findWhatsAppTab();
  const state = await callPageAgent(tab.id, 'focusComposer');
  if (!state.ok) throw new Error(state.error ?? 'WhatsApp composer focus failed.');
  await debuggerType(tab.id, message, false);
  const verified = await callPageAgent(tab.id, 'composerText', [message]);
  if (!verified.ok) throw new Error(`Draft verification failed. Composer contained: ${verified.actual ?? '<empty>'}`);
  return { tabId: tab.id, chatHeader: state.chatHeader, draftVerified: true, message };
}

async function sendInCurrentWhatsAppChat(message) {
  if (!message.trim()) throw new Error('The WhatsApp message is empty.');
  const tab = await findWhatsAppTab();
  const state = await callPageAgent(tab.id, 'focusComposer');
  if (!state.ok) throw new Error(state.error ?? 'WhatsApp composer focus failed.');
  await debuggerType(tab.id, message, true);
  const deadline = Date.now() + 6500;
  while (Date.now() < deadline) {
    const sent = await callPageAgent(tab.id, 'outgoingContains', [message]);
    if (sent.ok) return { tabId: tab.id, chatHeader: state.chatHeader, sentVerified: true, message };
    await delay(350);
  }
  throw new Error('Enter was dispatched, but the outgoing WhatsApp message could not be verified.');
}

async function debuggerType(tabId, text, pressEnter) {
  const target = { tabId };
  let attached = false;
  try {
    await chrome.debugger.attach(target, '1.3');
    attached = true;
    await dispatchKey(target, 'keyDown', 'a', 'KeyA', 65, 2);
    await dispatchKey(target, 'keyUp', 'a', 'KeyA', 65, 2);
    await dispatchKey(target, 'keyDown', 'Backspace', 'Backspace', 8, 0);
    await dispatchKey(target, 'keyUp', 'Backspace', 'Backspace', 8, 0);
    await chrome.debugger.sendCommand(target, 'Input.insertText', { text });
    if (pressEnter) {
      await delay(150);
      await dispatchKey(target, 'keyDown', 'Enter', 'Enter', 13, 0);
      await dispatchKey(target, 'keyUp', 'Enter', 'Enter', 13, 0);
    }
  } finally {
    if (attached) {
      try { await chrome.debugger.detach(target); } catch { /* no-op */ }
    }
  }
}

function dispatchKey(target, type, key, code, windowsVirtualKeyCode, modifiers) {
  return chrome.debugger.sendCommand(target, 'Input.dispatchKeyEvent', {
    type, key, code, windowsVirtualKeyCode, modifiers
  });
}

function delay(milliseconds) {
  return new Promise((resolve) => setTimeout(resolve, milliseconds));
}

connectLoop();
