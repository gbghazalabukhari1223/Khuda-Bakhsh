(() => {
  if (window.__kbJarvisPageAgent?.version === '13.0.0') return;

  const normalize = (value) => String(value ?? '')
    .toLowerCase()
    .replace(/[^\p{L}\p{N}]+/gu, ' ')
    .trim();

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
    return [...new Set(candidates)]
      .filter(isVisible)
      .filter((node) => node.getBoundingClientRect().top > window.innerHeight * 0.5)
      .sort((a, b) => b.getBoundingClientRect().top - a.getBoundingClientRect().top)[0] ?? null;
  };

  const findSearchBox = () => {
    const selectors = [
      '[data-testid="chat-list-search"] [contenteditable="true"]',
      '[data-testid="chat-list-search"] input',
      'div[role="textbox"][contenteditable="true"][data-tab="3"]',
      'div[role="textbox"][contenteditable="true"][aria-label*="search" i]',
      'input[placeholder*="search" i]',
      '[role="searchbox"]'
    ];
    return selectors
      .flatMap((selector) => [...document.querySelectorAll(selector)])
      .find((node) => isVisible(node) && node.getBoundingClientRect().top < window.innerHeight * 0.35) ?? null;
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
    const qrSelectors = ['canvas[aria-label*="QR" i]', '[data-ref] canvas', '[data-testid="qrcode"]'];
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
    return { ok: focused, chatHeader: readHeader(), tag: composer.tagName, role: composer.getAttribute('role') };
  };

  const focusSearch = () => {
    if (loginRequired()) return { ok: false, error: 'WhatsApp Web login QR is visible.' };
    const search = findSearchBox();
    if (!search) return { ok: false, error: 'WhatsApp search box was not found in the existing tab.' };
    search.click();
    search.focus();
    const active = document.activeElement;
    return {
      ok: active === search || search.contains(active),
      tag: search.tagName,
      role: search.getAttribute('role')
    };
  };

  const scoreLabel = (label, query) => {
    const normalizedLabel = normalize(label);
    const normalizedQuery = normalize(query);
    if (!normalizedLabel || !normalizedQuery) return 0;
    if (normalizedLabel === normalizedQuery) return 100;
    if (normalizedLabel.startsWith(normalizedQuery)) return 85;
    if (normalizedLabel.includes(normalizedQuery)) return 72;
    const tokens = normalizedQuery.split(' ').filter(Boolean);
    const matched = tokens.filter((token) => normalizedLabel.includes(token)).length;
    return tokens.length ? Math.round(matched / tokens.length * 60) : 0;
  };

  const collectContactCandidates = (query) => {
    const pane = document.querySelector('#pane-side') || document.body;
    const titled = [...pane.querySelectorAll('[title]')].filter(isVisible);
    const candidates = [];
    const seen = new Set();
    for (const node of titled) {
      const label = node.getAttribute('title') || node.textContent || '';
      const row = node.closest('[role="listitem"], [data-testid="cell-frame-container"], [tabindex="-1"]') || node.parentElement;
      if (!row || !isVisible(row) || seen.has(row)) continue;
      const score = scoreLabel(label, query);
      if (score <= 0) continue;
      seen.add(row);
      candidates.push({ row, label: label.trim(), score });
    }

    if (!candidates.length) {
      const rows = [...pane.querySelectorAll('[role="listitem"], [data-testid="cell-frame-container"]')].filter(isVisible);
      for (const row of rows) {
        const label = (row.innerText || row.textContent || '').split('\n')[0].trim();
        const score = scoreLabel(label, query);
        if (score > 0) candidates.push({ row, label, score });
      }
    }
    return candidates.sort((a, b) => b.score - a.score);
  };

  const openBestContact = (query) => {
    const candidates = collectContactCandidates(query);
    if (!candidates.length || candidates[0].score < 45) {
      return { ok: false, error: `No reliable WhatsApp contact match was found for “${query}”.`, candidates: [] };
    }
    const best = candidates[0];
    best.row.scrollIntoView({ block: 'center', inline: 'nearest' });
    best.row.click();
    return {
      ok: true,
      selectedLabel: best.label,
      score: best.score,
      alternatives: candidates.slice(1, 4).map((candidate) => candidate.label)
    };
  };

  const headerMatches = (expected) => {
    const header = readHeader();
    const score = scoreLabel(header, expected);
    return { ok: score >= 45, chatHeader: header, score };
  };

  const composerText = (expected) => {
    const composer = findComposer();
    if (!composer) return { ok: false, actual: null };
    const actual = (composer.innerText || composer.textContent || '').replace(/\u00a0/g, ' ').trim();
    return { ok: actual === String(expected).trim(), actual };
  };

  const outgoingContains = (expected) => {
    const normalized = String(expected).trim();
    const outgoing = [...document.querySelectorAll('[data-testid="msg-container"], .message-out')].filter(isVisible);
    return { ok: outgoing.some((node) => (node.innerText || node.textContent || '').includes(normalized)) };
  };

  window.__kbJarvisPageAgent = {
    version: '13.0.0',
    inspect,
    focusComposer,
    focusSearch,
    openBestContact,
    headerMatches,
    composerText,
    outgoingContains,
    readHeader
  };
})();