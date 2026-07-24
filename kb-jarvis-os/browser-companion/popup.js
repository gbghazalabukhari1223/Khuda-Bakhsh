const state = document.getElementById('state');
const detail = document.getElementById('detail');
const reconnect = document.getElementById('reconnect');

async function refresh() {
  const status = await chrome.runtime.sendMessage({ type: 'status' });
  state.textContent = status?.connected ? 'CONNECTED' : 'NOT CONNECTED';
  state.className = status?.connected ? 'online' : 'offline';
  detail.textContent = status?.connected
    ? `Native WebSocket active on port ${status.port}`
    : 'Start KB Jarvis OS. The companion will reconnect automatically.';
}

reconnect.addEventListener('click', async () => {
  await chrome.runtime.sendMessage({ type: 'reconnect' });
  setTimeout(refresh, 700);
});

refresh();
setInterval(refresh, 1500);
