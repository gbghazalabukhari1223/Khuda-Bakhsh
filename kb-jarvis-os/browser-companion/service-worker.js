const VERSION = '14.0.0';
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
    sendResponse({ connected: socket?.readyState === WebSocket.OPEN, port: activePort, version: VERSION, state: connectionState, detail: connectionDetail });
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
    try { candidate = new WebSocket(`ws://127.0.0.1:${port}/ws`); }
    catch { finish(null); return; }
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
    case 'youtube.play': return await playYouTube(String(payload.query ?? ''));
    case 'chatgpt.latest_response': return await readLatestChatGptResponse();
    case 'wordpress.content': return await operateWordPressContent(payload);
    default: throw new Error(`Unsupported Browser Companion operation: ${operation}`);
  }
}

async function listTabs() {
  const tabs = await chrome.tabs.query({});
  return tabs.map((tab) => ({ id: tab.id, title: tab.title, url: tab.url, active: tab.active, windowId: tab.windowId }));
}

async function findWhatsAppTab() {
  const tabs = await chrome.tabs.query({ url: ['https://web.whatsapp.com/*'] });
  if (!tabs.length) throw new Error('No existing WhatsApp Web tab was found. Jarvis will not create a duplicate or logged-out tab. Open and sign in to WhatsApp Web once.');
  const selected = tabs.find((tab) => tab.active) ?? tabs[0];
  await focusTab(selected);
  await ensurePageAgent(selected.id);
  return selected;
}
async function ensurePageAgent(tabId) {
  await chrome.scripting.executeScript({ target: { tabId }, world: 'MAIN', files: ['page-agent.js'] });
}
async function callPageAgent(tabId, method, args = []) {
  const [result] = await chrome.scripting.executeScript({
    target: { tabId }, world: 'MAIN', args: [method, args],
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
  await delay(850);
  const selected = await callPageAgent(tab.id, 'openBestContact', [contact]);
  if (!selected.ok) throw new Error(selected.error ?? `No reliable contact match was found for ${contact}.`);
  const deadline = Date.now() + 5000;
  while (Date.now() < deadline) {
    const matched = await callPageAgent(tab.id, 'headerMatches', [selected.selectedLabel || contact]);
    if (matched.ok) return { tabId: tab.id, chatHeader: matched.chatHeader, selectedLabel: selected.selectedLabel, score: selected.score, alternatives: selected.alternatives };
    await delay(250);
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
  const deadline = Date.now() + 6500;
  while (Date.now() < deadline) {
    const sent = await callPageAgent(tab.id, 'outgoingContains', [message]);
    if (sent.ok) return { tabId: tab.id, chatHeader: state.chatHeader, sentVerified: true, message };
    await delay(300);
  }
  throw new Error('Enter was dispatched, but the outgoing WhatsApp message could not be verified.');
}

async function playYouTube(query) {
  if (!query.trim()) throw new Error('A YouTube search query is required.');
  const searchUrl = `https://www.youtube.com/results?search_query=${encodeURIComponent(query)}`;
  const tabs = await chrome.tabs.query({ url: ['https://www.youtube.com/*'] });
  let tab = tabs.find((candidate) => candidate.active) ?? tabs[0];
  if (tab) tab = await chrome.tabs.update(tab.id, { url: searchUrl, active: true });
  else tab = await chrome.tabs.create({ url: searchUrl, active: true });
  await chrome.windows.update(tab.windowId, { focused: true });
  await waitForTabComplete(tab.id, 15000);

  const [selection] = await chrome.scripting.executeScript({
    target: { tabId: tab.id },
    func: () => {
      const visible = (node) => {
        const rect = node.getBoundingClientRect();
        return rect.width > 20 && rect.height > 10;
      };
      const links = [...document.querySelectorAll('a#video-title[href*="/watch"], ytd-video-renderer a[href*="/watch"]')]
        .filter(visible)
        .map((node) => ({ href: node.href, title: (node.getAttribute('title') || node.textContent || '').trim() }))
        .filter((item) => item.href && item.title);
      return links[0] ?? null;
    }
  });
  const selected = selection?.result;
  if (!selected?.href) throw new Error(`No normal YouTube video result was found for “${query}”.`);
  await chrome.tabs.update(tab.id, { url: selected.href, active: true });
  await waitForTabComplete(tab.id, 15000);
  await delay(900);

  let state = await getYouTubeState(tab.id);
  if (!state.playing) {
    const point = await getVideoCenter(tab.id);
    if (point) await debuggerClick(tab.id, point.x, point.y);
    await delay(500);
    await chrome.scripting.executeScript({ target: { tabId: tab.id }, func: () => document.querySelector('video')?.play().catch(() => {}) });
    await delay(1100);
    state = await getYouTubeState(tab.id);
  }
  return { tabId: tab.id, title: state.title || selected.title, url: selected.href, playing: state.playing, currentTime: state.currentTime };
}
async function getYouTubeState(tabId) {
  const [result] = await chrome.scripting.executeScript({
    target: { tabId },
    func: () => {
      const video = document.querySelector('video');
      const title = document.querySelector('h1 yt-formatted-string, ytd-watch-metadata h1, meta[name="title"]')?.textContent?.trim()
        || document.querySelector('meta[name="title"]')?.getAttribute('content')
        || document.title.replace(/\s*-\s*YouTube\s*$/, '');
      return { title, playing: Boolean(video && !video.paused && !video.ended), currentTime: Number(video?.currentTime || 0) };
    }
  });
  return result?.result ?? { title: '', playing: false, currentTime: 0 };
}
async function getVideoCenter(tabId) {
  const [result] = await chrome.scripting.executeScript({
    target: { tabId },
    func: () => {
      const video = document.querySelector('video');
      if (!video) return null;
      const rect = video.getBoundingClientRect();
      return { x: Math.round(rect.left + rect.width / 2), y: Math.round(rect.top + rect.height / 2) };
    }
  });
  return result?.result ?? null;
}

async function readLatestChatGptResponse() {
  const tabs = await chrome.tabs.query({ url: ['https://chatgpt.com/*'] });
  if (!tabs.length) throw new Error('No existing ChatGPT tab was found.');
  const tab = tabs.find((candidate) => candidate.active) ?? tabs[0];
  await focusTab(tab);
  const [result] = await chrome.scripting.executeScript({
    target: { tabId: tab.id },
    func: () => {
      const messages = [...document.querySelectorAll('[data-message-author-role="assistant"]')];
      const latest = messages.at(-1);
      const text = (latest?.innerText || latest?.textContent || '').trim();
      return { text, length: text.length };
    }
  });
  const data = result?.result;
  if (!data?.text) throw new Error('The existing ChatGPT tab has no readable assistant response.');
  return { tabId: tab.id, text: data.text, length: data.length };
}

async function operateWordPressContent(payload) {
  const action = String(payload.action ?? 'create_post').toLowerCase();
  const title = String(payload.title ?? '');
  const content = String(payload.content ?? '');
  const status = String(payload.status ?? 'draft').toLowerCase();
  if (!content.trim()) throw new Error('WordPress content is empty.');

  const tabs = await chrome.tabs.query({ url: ['*://*/wp-admin/*'] });
  if (!tabs.length) throw new Error('No existing signed-in WordPress wp-admin tab was found.');
  let tab = tabs.find((candidate) => candidate.active) ?? tabs[0];
  await focusTab(tab);

  if (action === 'create_page' || action === 'create_post') {
    const current = new URL(tab.url);
    const target = action === 'create_page'
      ? `${current.origin}/wp-admin/post-new.php?post_type=page`
      : `${current.origin}/wp-admin/post-new.php`;
    tab = await chrome.tabs.update(tab.id, { url: target, active: true });
    await waitForTabComplete(tab.id, 16000);
    await delay(1100);
  }

  const prepared = await prepareWordPressEditor(tab.id, title, content);
  if (!prepared.ok) throw new Error(prepared.error ?? 'WordPress editor fields could not be prepared.');
  const saved = await saveWordPressEditor(tab.id, status);
  return {
    tabId: tab.id,
    title: prepared.title || title,
    editor: prepared.editor,
    url: tab.url,
    saved: Boolean(saved.saved),
    published: Boolean(saved.published),
    evidence: saved.evidence
  };
}

async function prepareWordPressEditor(tabId, title, content) {
  const [result] = await chrome.scripting.executeScript({
    target: { tabId },
    args: [title, content],
    func: (requestedTitle, requestedContent) => {
      const visible = (node) => {
        if (!(node instanceof Element)) return false;
        const rect = node.getBoundingClientRect();
        const style = getComputedStyle(node);
        return rect.width > 2 && rect.height > 2 && style.display !== 'none' && style.visibility !== 'hidden';
      };
      const setNativeValue = (node, value) => {
        node.focus();
        if ('value' in node) {
          const prototype = node.tagName === 'TEXTAREA' ? HTMLTextAreaElement.prototype : HTMLInputElement.prototype;
          const setter = Object.getOwnPropertyDescriptor(prototype, 'value')?.set;
          if (setter) setter.call(node, value); else node.value = value;
        } else {
          node.textContent = value;
        }
        node.dispatchEvent(new InputEvent('input', { bubbles: true, inputType: 'insertText', data: value }));
        node.dispatchEvent(new Event('change', { bubbles: true }));
        node.blur();
      };

      const titleSelectors = [
        'h1.editor-post-title__input[contenteditable="true"]',
        '.editor-post-title__input[contenteditable="true"]',
        'textarea.editor-post-title__input',
        'input[name="post_title"]',
        '#title'
      ];
      const titleNode = titleSelectors.flatMap((selector) => [...document.querySelectorAll(selector)]).find(visible);
      if (requestedTitle && titleNode) setNativeValue(titleNode, requestedTitle);

      let contentNode = [
        '.block-editor-rich-text__editable[contenteditable="true"]',
        '.wp-block-post-content [contenteditable="true"]',
        '[data-type="core/paragraph"] [contenteditable="true"]',
        'textarea[name="content"]',
        '#content'
      ].flatMap((selector) => [...document.querySelectorAll(selector)]).find(visible);

      if (!contentNode) {
        const appender = [...document.querySelectorAll('.block-editor-default-block-appender__content, button[aria-label*="Add default block" i], button[aria-label*="Type / to choose a block" i]')].find(visible);
        appender?.click();
        contentNode = [...document.querySelectorAll('.block-editor-rich-text__editable[contenteditable="true"], .wp-block-post-content [contenteditable="true"]')].find(visible);
      }
      if (!contentNode) return { ok: false, error: 'No Gutenberg or classic WordPress content editor was visible.' };
      setNativeValue(contentNode, requestedContent);
      const titleText = (titleNode?.value || titleNode?.innerText || titleNode?.textContent || requestedTitle || '').trim();
      const actualContent = (contentNode.value || contentNode.innerText || contentNode.textContent || '').trim();
      return { ok: actualContent.length > 0, title: titleText, editor: contentNode.tagName === 'TEXTAREA' ? 'classic' : 'block', contentLength: actualContent.length };
    }
  });
  return result?.result ?? { ok: false, error: 'WordPress editor preparation returned no result.' };
}

async function saveWordPressEditor(tabId, status) {
  const [result] = await chrome.scripting.executeScript({
    target: { tabId },
    args: [status],
    func: (requestedStatus) => {
      const visible = (node) => {
        const rect = node.getBoundingClientRect();
        return rect.width > 2 && rect.height > 2 && getComputedStyle(node).display !== 'none';
      };
      const byText = (patterns) => [...document.querySelectorAll('button, input[type="submit"]')].find((node) => {
        const text = (node.innerText || node.value || node.getAttribute('aria-label') || '').trim().toLowerCase();
        return visible(node) && patterns.some((pattern) => text.includes(pattern));
      });
      if (requestedStatus === 'publish') {
        const first = document.querySelector('.editor-post-publish-panel__toggle, .editor-post-publish-button__button') || byText(['publish']);
        if (!first) return { clicked: false, error: 'Publish button was not found.' };
        first.click();
        return { clicked: true, phase: 'publish-opened' };
      }
      const draft = document.querySelector('.editor-post-save-draft, #save-post') || byText(['save draft', 'save as pending']);
      if (!draft) return { clicked: false, error: 'Save draft button was not found.' };
      draft.click();
      return { clicked: true, phase: 'draft' };
    }
  });
  const initial = result?.result;
  if (!initial?.clicked) throw new Error(initial?.error ?? 'WordPress save control could not be clicked.');
  await delay(1200);

  if (status === 'publish') {
    const [confirmation] = await chrome.scripting.executeScript({
      target: { tabId },
      func: () => {
        const buttons = [...document.querySelectorAll('button')];
        const publish = buttons.find((node) => {
          const text = (node.innerText || node.getAttribute('aria-label') || '').trim().toLowerCase();
          const rect = node.getBoundingClientRect();
          return rect.width > 2 && rect.height > 2 && (text === 'publish' || text.includes('publish now'));
        });
        if (!publish) return { clicked: false };
        publish.click();
        return { clicked: true };
      }
    });
    if (!confirmation?.result?.clicked) throw new Error('The final WordPress publish confirmation button was not found.');
    await delay(1800);
  } else {
    await delay(800);
  }

  const [verification] = await chrome.scripting.executeScript({
    target: { tabId },
    args: [status],
    func: (requestedStatus) => {
      const bodyText = document.body?.innerText?.toLowerCase() || '';
      const saved = bodyText.includes('draft saved') || bodyText.includes('saved') || Boolean(document.querySelector('.editor-post-saved-state.is-saved'));
      const published = bodyText.includes('post published') || bodyText.includes('page published') || bodyText.includes('published.');
      return { saved: requestedStatus === 'draft' ? saved : published, published, evidence: published ? 'Published confirmation detected.' : saved ? 'Saved state detected.' : 'Editor action completed; confirmation text not detected.' };
    }
  });
  return verification?.result ?? { saved: false, published: false, evidence: 'No verification result.' };
}

async function focusTab(tab) {
  await chrome.tabs.update(tab.id, { active: true });
  if (tab.windowId !== undefined) await chrome.windows.update(tab.windowId, { focused: true });
}
async function waitForTabComplete(tabId, timeoutMs) {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    const tab = await chrome.tabs.get(tabId);
    if (tab.status === 'complete') return tab;
    await delay(200);
  }
  throw new Error('Browser page loading timed out.');
}
async function debuggerClick(tabId, x, y) {
  const target = { tabId };
  let attached = false;
  try {
    await chrome.debugger.attach(target, '1.3');
    attached = true;
    await chrome.debugger.sendCommand(target, 'Input.dispatchMouseEvent', { type: 'mousePressed', x, y, button: 'left', clickCount: 1 });
    await chrome.debugger.sendCommand(target, 'Input.dispatchMouseEvent', { type: 'mouseReleased', x, y, button: 'left', clickCount: 1 });
  } finally {
    if (attached) try { await chrome.debugger.detach(target); } catch { }
  }
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
      await delay(120);
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