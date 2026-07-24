const state = document.getElementById('state');
const detail = document.getElementById('detail');
const reconnect = document.getElementById('reconnect');

async function refresh() {
  try {
    const status = await chrome.runtime.sendMessage({ type: 'status' });
    const connected = Boolean(status?.connected);
    state.textContent = connected
      ? 'CONNECTED'
      : status?.state === 'probing'
        ? 'SEARCHING'
        : 'JARVIS APP OFFLINE';
    state.className = connected ? 'online' : 'offline';
    detail.textContent = status?.detail
      ?? (connected
        ? `Native WebSocket active on port ${status.port}`
        : 'Start KB Jarvis OS. The companion will reconnect automatically.');
  } catch {
    state.textContent = 'EXTENSION RESTARTING';
    state.className = 'offline';
    detail.textContent = 'Reload the extension once, then start KB Jarvis OS.';
  }
}

reconnect.addEventListener('click', async () => {
  reconnect.disabled = true;
  try {
    await chrome.runtime.sendMessage({ type: 'reconnect' });
    setTimeout(refresh, 850);
  } finally {
    setTimeout(() => { reconnect.disabled = false; }, 1000);
  }
});

refresh();
setInterval(refresh, 1500);