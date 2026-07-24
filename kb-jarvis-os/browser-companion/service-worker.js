const VERSION = '13.0.0';
const PORTS = Array.from({ length: 11 }, (_, index) => 32145 + index);
const RECONNECT_DELAY_MS = 2200;
const HEALTH_TIMEOUT_MS = 550;
const SOCKET_TIMEOUT_MS = 1600;

let socket = null;
let activePort = null;
let connecting = false;
let connectionState = 'offline';
let connectionDetail = 'Start KB Jarvis OS. The companion will reconnect automatically.';
let reconnectTimer = null;

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
    sendResponse({
      connected: socket?.readyState === WebSocket.OPEN,
      port: activePort,
      version: VERSION,
      state: connectionState,
      detail: connectionDetail
    });
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
  clearReconnectTimer();
  connectionState = 'probing';
  connectionDetail = 'Locating the running KB Jarvis native bridge…';
  try {
    closeCurrentSocket();
    const stored = await chrome.storage.local.get(['activePort']);
    const rememberedPort = Number(stored?.activePort);
    const orderedPorts = Number.isInteger(rememberedPort) && PORTS.includes(rememberedPort)
      ? [rememberedPort, ...PORTS.filter((port) => port !== rememberedPort)]
      : PORTS;
    for (const port of orderedPorts) {
      const health = await probeHealth(port);
      if (!health?.healthy) continue;
      const candidate = await connectToPort(port);
      if (!candidate) continue;
      socket = candidate;
      activePort = port;
      connectionState = 'connected';
      connectionDetail = `Connected to KB Jarvis OS on port ${port}.`;
      await chrome.storage.local.set({ activePort: port, lastConnectedAt: Date.now(), lastNativeVersion: health.version ?? null });
      return;
    }
    activePort = null;
    connectionState = 'offline';
    connectionDetail = 'KB Jarvis OS is not running or its native bridge has not started yet.';
  } catch (error) {
    activePort = null;
    connectionState = 'offline';
    connectionDetail = error instanceof Error ? error.message : String(error);
  } finally {
    connecting = false;
  }
  scheduleReconnect();
}

async function probeHealth(port) {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), HEALTH_TIMEOUT_MS);
  try {
    const response = await fetch(`http://127.0.0.1:${port}/health`, { method: 'GET', cache: 'no-store', signal: controller.signal });
    if (!response.ok) return { healthy: false };
    const data = await response.json().catch(() => ({}));
    const recognized = String(data?.name ?? '').toLowerCase().includes('kb jarvis');
    return { healthy: recognized && String(data?.status ?? '').toLowerCase() === 'ok', version: data?.version ?? null };
  } catch {
    return { healthy: false };
  } finally {
    clearTimeout(timeout);
  }
}

function connectToPort(port) {
  return new Promise((resolve) => {
    let settled = false;
    let candidate;
    const finish = (value) => {
      if (settled) return;
      settled = true;
      clearTimeout(timeout);
      resolve(value);
    };
    const timeout = setTimeout(() => {
      try { candidate?.close(); } catch { }
      finish(null);
    }, SOCKET_TIMEOUT_MS);
    try {
      candidate = new WebSocket(`ws://127.0.0.1:${port}/ws`);
    } catch {
      finish(null);
      return;
    }
    candidate.onopen = () => {
      candidate.onmessage = (event) => handleCommandMessage(event.data, candidate);
      candidate.onclose = () => handleSocketClosed(candidate);
      candidate.onerror = () => { };
      candidate.send(JSON.stringify({ type: 'hello', product: 'KB Jarvis OS Browser Companion', version: VERSION, developer: 'KB (Khuda Bakhsh)' }));
      finish(candidate);
    };
    candidate.onerror = () => finish(null);
    candidate.onclose = () => finish(null);
  });
}

function handleSocketClosed(candidate) {
  if (socket !== candidate) return;
  socket = null;
  activePort = null;
  connectionState = 'offline';
  connectionDetail = 'The native bridge disconnected. Reconnecting automatically…';
  scheduleReconnect();
}
function closeCurrentSocket() {
  const current = socket;
  socket = null;
  activePort = null;
  if (current) try { current.close(); } catch { }
}
function scheduleReconnect() {
  clearReconnectTimer();
  reconnectTimer = setTimeout(() => connectLoop(), RECONNECT_DELAY_MS);
}
function clearReconnectTimer() {
  if (reconnectTimer) {
    clearTimeout(reconnectTimer);
    reconnectTimer = null;
  }
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
  if (channel.readyState === WebSocket.OPEN) channel.send(JSON.stringify({ type: 'result', id, success, data, error }));
}

async function executeOperation(operation, payload) {
  switch (operation) {
    case 'tabs.list': return await listTabs();
    case 'whatsapp.contact.open': return await openWhatsAppContact(String(payload.contact ?? ''));
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
  if (!tabs.length) {
    throw new Error('No existing WhatsApp Web tab was found. Jarvis will not create a duplicate or logged-out tab. Open and sign in to WhatsApp Web once.');
  }
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
      if (!agent || typeof agent[methodName] !== 'function') return { ok: false, error: 'KB Jarvis page agent is not available.' };
      return agent[methodName](...methodArgs);
    }
  });
  return result?.result ?? { ok: false, error: 'The page agent returned no result.' };
}

async function openWhatsAppContact(contact) {
  if (!contact.trim()) throw new Error('A WhatsApp contact name is required.');
  const tab = await findWhatsAppTab();
  const focused = await callPageAgent(tab.id, 'focusSearch');
  if (!focused.ok) throw new Error(focused.error ?? 'WhatsApp contact search could not be focused.');
  await debuggerType(tab.id, contact, false);
  await delay(1000);
  const selected = await callPageAgent(tab.id, 'openBestContact', [contact]);
  if (!selected.ok) throw new Error(selected.error ?? `No reliable contact match was found for ${contact}.`);

  const deadline = Date.now() + 5500;
  while (Date.now() < deadline) {
    const matched = await callPageAgent(tab.id, 'headerMatches', [selected.selectedLabel || contact]);
    if (matched.ok) {
      return {
        tabId: tab.id,
        chatHeader: matched.chatHeader,
        selectedLabel: selected.selectedLabel,
        score: selected.score,
        alternatives: selected.alternatives
      };
    }
    await delay(300);
  }
  throw new Error(`A contact candidate was clicked, but the chat header could not be verified for “${contact}”.`);
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
  const deadline = Date.now() + 7000;
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
    if (attached) try { await chrome.debugger.detach(target); } catch { }
  }
}
function dispatchKey(target, type, key, code, windowsVirtualKeyCode, modifiers) {
  return chrome.debugger.sendCommand(target, 'Input.dispatchKeyEvent', { type, key, code, windowsVirtualKeyCode, modifiers });
}
function delay(milliseconds) {
  return new Promise((resolve) => setTimeout(resolve, milliseconds));
}

connectLoop();