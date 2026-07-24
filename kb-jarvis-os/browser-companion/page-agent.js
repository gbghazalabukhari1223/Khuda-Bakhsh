(() => {
  if (window.__kbJarvisPageAgent?.version === '11.0.0') return;

  const isVisible = (node) => {
    if (!(node instanceof Element)) return false;
    const rect = node.getBoundingClientRect();
    const style = getComputedStyle(node);
    return rect.width > 2
      && rect.height > 2
      && style.visibility !== 'hidden'
      && style.display !== 'none'
      && Number(style.opacity || 1) > 0;
  };

  const findComposer = () => {
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
      .filter((node) => node.getBoundingClientRect().top > window.innerHeight * 0.52)
      .sort((a, b) => b.getBoundingClientRect().top - a.getBoundingClientRect().top)[0] ?? null;
  };

  const readHeader = () => {
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
  };

  const loginRequired = () => {
    const qrSelectors = [
      'canvas[aria-label*="QR" i]',
      '[data-ref] canvas',
      '[data-testid="qrcode"]'
    ];
    return qrSelectors.some((selector) => [...document.querySelectorAll(selector)].some(isVisible));
  };

  const inspect = () => {
    if (loginRequired()) return { ok: false, error: 'WhatsApp Web login QR is visible.' };
    const composer = findComposer();
    if (!composer) return { ok: false, error: 'No visible current-chat message composer was found.' };
    return { ok: true, chatHeader: readHeader(), composerFound: true };
  };

  const focusComposer = () => {
    if (loginRequired()) return { ok: false, error: 'WhatsApp Web login QR is visible.' };
    const composer = findComposer();
    if (!composer) return { ok: false, error: 'No visible message composer was found in the current WhatsApp chat.' };
    composer.scrollIntoView({ block: 'center', inline: 'nearest' });
    composer.click();
    composer.focus();
    const active = document.activeElement;
    const focused = active === composer || composer.contains(active);
    return {
      ok: focused,
      chatHeader: readHeader(),
      tag: composer.tagName,
      role: composer.getAttribute('role')
    };
  };

  const composerText = (expected) => {
    const composer = findComposer();
    if (!composer) return { ok: false, actual: null };
    const actual = (composer.innerText || composer.textContent || '').replace(/\u00a0/g, ' ').trim();
    return { ok: actual === String(expected).trim(), actual };
  };

  const outgoingContains = (expected) => {
    const normalized = String(expected).trim();
    const outgoing = [...document.querySelectorAll('[data-testid="msg-container"], .message-out')]
      .filter(isVisible);
    return {
      ok: outgoing.some((node) => (node.innerText || node.textContent || '').includes(normalized))
    };
  };

  window.__kbJarvisPageAgent = {
    version: '11.0.0',
    inspect,
    focusComposer,
    composerText,
    outgoingContains,
    readHeader
  };
})();
