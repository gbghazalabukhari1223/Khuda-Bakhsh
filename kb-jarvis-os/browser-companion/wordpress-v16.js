async function operateWordPressContentV16(payload) {
  const action = String(payload?.action ?? 'create_post').toLowerCase();
  const title = String(payload?.title ?? '');
  const content = String(payload?.content ?? '');
  const status = String(payload?.status ?? 'draft').toLowerCase();
  if (!content.trim()) throw new Error('WordPress content is empty.');

  const tabs = await chrome.tabs.query({ url: ['*://*/wp-admin/*'] });
  if (!tabs.length) {
    throw new Error('No existing signed-in WordPress wp-admin tab was found. Open the correct site and sign in once.');
  }

  const lastWindow = await chrome.windows.getLastFocused().catch(() => null);
  const focusedCandidates = lastWindow
    ? tabs.filter((candidate) => candidate.windowId === lastWindow.id)
    : [];
  let tab = focusedCandidates.find((candidate) => candidate.active)
    ?? tabs.find((candidate) => candidate.active)
    ?? (tabs.length === 1 ? tabs[0] : null);

  if (!tab) {
    const sites = [...new Set(tabs.map((candidate) => {
      try { return new URL(candidate.url).origin; } catch { return candidate.url; }
    }))];
    throw new Error(`Several WordPress admin sessions are open. Activate the intended site first: ${sites.join(', ')}`);
  }

  await focusTab(tab);
  if (action === 'create_page' || action === 'create_post') {
    const current = new URL(tab.url);
    const target = action === 'create_page'
      ? `${current.origin}/wp-admin/post-new.php?post_type=page`
      : `${current.origin}/wp-admin/post-new.php`;
    tab = await chrome.tabs.update(tab.id, { url: target, active: true });
    await waitForTabComplete(tab.id, 20000);
    await delay(900);
  }

  const inspection = await inspectWordPressV16(tab.id);
  if (!inspection.authenticated) {
    throw new Error(`WordPress authentication is not active on ${inspection.site || tab.url}. Sign in and leave the wp-admin editor tab open.`);
  }

  if (inspection.editorApiAvailable) {
    try {
      const prepared = await prepareWordPressViaDataV16(tab.id, title, content);
      if (!prepared.ok) throw new Error(prepared.error || 'The WordPress editor data store did not accept the requested content.');
      const saved = await saveWordPressViaDataV16(tab.id, status);
      if (!saved.ok) throw new Error(saved.error || 'WordPress did not confirm the save operation.');
      const verified = await verifyWordPressViaDataV16(tab.id, title, content, status);
      if (!verified.ok) throw new Error(verified.error || 'WordPress editor state did not match the requested result after saving.');

      return {
        tabId: tab.id,
        site: inspection.site,
        title: verified.title || prepared.title || title,
        editor: 'gutenberg-data-api',
        postId: verified.postId || saved.postId || null,
        postType: verified.postType || inspection.postType || null,
        status: verified.status,
        url: verified.permalink || tab.url,
        permalink: verified.permalink || null,
        modified: verified.modified || null,
        contentLength: verified.contentLength,
        contentMatch: verified.contentMatch,
        saved: status === 'draft' ? verified.status === 'draft' || verified.status === 'pending' || verified.status === 'private' : verified.status === 'publish',
        published: verified.status === 'publish',
        evidence: `WordPress editor data API verified post ${verified.postId ?? '<new>'}, status ${verified.status}, title and content state on ${inspection.site}.`
      };
    } catch (error) {
      console.warn('KB Jarvis WordPress data API path failed; using preserved DOM fallback.', error);
    }
  }

  const legacy = await operateWordPressContent(payload);
  return {
    ...legacy,
    site: inspection.site,
    editor: legacy.editor || 'preserved-dom-fallback',
    evidence: legacy.evidence
      ? `${legacy.evidence} Site: ${inspection.site}. DOM fallback was used because the direct editor data API was unavailable or did not verify.`
      : `Site: ${inspection.site}. Preserved DOM fallback completed.`
  };
}

async function inspectWordPressV16(tabId) {
  const [result] = await chrome.scripting.executeScript({
    target: { tabId },
    world: 'MAIN',
    func: () => {
      const loginVisible = Boolean(document.querySelector('#loginform, body.login, form[name="loginform"]'));
      const editorSelect = window.wp?.data?.select?.('core/editor');
      let title = '';
      let status = '';
      let postId = null;
      let postType = null;
      let dirty = null;
      let saving = null;
      try {
        title = editorSelect?.getEditedPostAttribute?.('title') || editorSelect?.getCurrentPostAttribute?.('title') || '';
        status = editorSelect?.getEditedPostAttribute?.('status') || editorSelect?.getCurrentPostAttribute?.('status') || '';
        postId = editorSelect?.getCurrentPostId?.() ?? null;
        postType = editorSelect?.getCurrentPostType?.() ?? null;
        dirty = editorSelect?.isEditedPostDirty?.() ?? null;
        saving = editorSelect?.isSavingPost?.() ?? null;
      } catch { }
      return {
        site: location.origin,
        authenticated: !loginVisible && location.pathname.includes('/wp-admin/'),
        editorApiAvailable: Boolean(window.wp?.data?.dispatch?.('core/editor') && editorSelect),
        title,
        status,
        postId,
        postType,
        dirty,
        saving,
        documentTitle: document.title
      };
    }
  });
  return result?.result ?? {
    site: '', authenticated: false, editorApiAvailable: false
  };
}

async function prepareWordPressViaDataV16(tabId, requestedTitle, requestedContent) {
  const [result] = await chrome.scripting.executeScript({
    target: { tabId },
    world: 'MAIN',
    args: [requestedTitle, requestedContent],
    func: async (title, content) => {
      try {
        const wp = window.wp;
        const editorDispatch = wp?.data?.dispatch?.('core/editor');
        const editorSelect = wp?.data?.select?.('core/editor');
        if (!editorDispatch || !editorSelect) return { ok: false, error: 'WordPress core/editor data store is unavailable.' };

        if (title) editorDispatch.editPost({ title });

        const blockDispatch = wp?.data?.dispatch?.('core/block-editor');
        const blockSelect = wp?.data?.select?.('core/block-editor');
        let usedBlocks = false;
        if (blockDispatch?.resetBlocks && wp?.blocks) {
          let blocks = [];
          try {
            blocks = typeof wp.blocks.rawHandler === 'function'
              ? wp.blocks.rawHandler({ HTML: content })
              : typeof wp.blocks.parse === 'function'
                ? wp.blocks.parse(content)
                : [];
          } catch { blocks = []; }
          if (Array.isArray(blocks) && blocks.length) {
            blockDispatch.resetBlocks(blocks);
            usedBlocks = true;
          }
        }
        if (!usedBlocks) editorDispatch.editPost({ content });

        await new Promise((resolve) => setTimeout(resolve, 180));
        const actualTitle = String(editorSelect.getEditedPostAttribute?.('title') || '').trim();
        let actualContent = String(editorSelect.getEditedPostAttribute?.('content') || '');
        if (blockSelect?.getBlocks && wp?.blocks?.serialize) {
          const blocks = blockSelect.getBlocks();
          if (Array.isArray(blocks) && blocks.length) actualContent = wp.blocks.serialize(blocks);
        }
        const normalise = (value) => String(value || '')
          .replace(/<!--\s*\/?wp:[^>]*-->/g, ' ')
          .replace(/<[^>]+>/g, ' ')
          .replace(/&nbsp;/gi, ' ')
          .replace(/\s+/g, ' ')
          .trim();
        const requestedNormal = normalise(content);
        const actualNormal = normalise(actualContent);
        const contentMatch = requestedNormal.length === 0
          ? actualNormal.length === 0
          : actualNormal.includes(requestedNormal) || requestedNormal.includes(actualNormal);
        return {
          ok: Boolean(actualContent.trim()) && (!title || actualTitle === String(title).trim()) && contentMatch,
          title: actualTitle,
          contentLength: actualContent.length,
          contentMatch,
          usedBlocks
        };
      } catch (error) {
        return { ok: false, error: error instanceof Error ? error.message : String(error) };
      }
    }
  });
  return result?.result ?? { ok: false, error: 'WordPress editor preparation returned no result.' };
}

async function saveWordPressViaDataV16(tabId, requestedStatus) {
  const [result] = await chrome.scripting.executeScript({
    target: { tabId },
    world: 'MAIN',
    args: [requestedStatus],
    func: async (status) => {
      try {
        const dispatch = window.wp?.data?.dispatch?.('core/editor');
        const select = window.wp?.data?.select?.('core/editor');
        if (!dispatch || !select?.isSavingPost) return { ok: false, error: 'WordPress save data API is unavailable.' };

        if (status === 'publish') dispatch.editPost({ status: 'publish' });
        else {
          const current = select.getEditedPostAttribute?.('status') || select.getCurrentPostAttribute?.('status');
          if (!current || current === 'auto-draft' || current === 'publish') dispatch.editPost({ status: 'draft' });
        }

        const savePromise = dispatch.savePost?.();
        if (savePromise?.then) await savePromise;

        const deadline = Date.now() + 22000;
        while (Date.now() < deadline && select.isSavingPost()) {
          await new Promise((resolve) => setTimeout(resolve, 150));
        }
        const error = select.getLastPostSaveError?.();
        const postId = select.getCurrentPostId?.() ?? null;
        const finalStatus = select.getEditedPostAttribute?.('status') || select.getCurrentPostAttribute?.('status') || '';
        const permalink = select.getCurrentPostAttribute?.('link') || select.getPermalink?.() || '';
        return {
          ok: !select.isSavingPost() && !error && Boolean(postId) && (status !== 'publish' || finalStatus === 'publish'),
          postId,
          status: finalStatus,
          permalink,
          error: error ? JSON.stringify(error) : null
        };
      } catch (error) {
        return { ok: false, error: error instanceof Error ? error.message : String(error) };
      }
    }
  });
  return result?.result ?? { ok: false, error: 'WordPress save returned no result.' };
}

async function verifyWordPressViaDataV16(tabId, requestedTitle, requestedContent, requestedStatus) {
  const [result] = await chrome.scripting.executeScript({
    target: { tabId },
    world: 'MAIN',
    args: [requestedTitle, requestedContent, requestedStatus],
    func: (title, content, requestedStatus) => {
      try {
        const wp = window.wp;
        const editor = wp?.data?.select?.('core/editor');
        const blocks = wp?.data?.select?.('core/block-editor');
        if (!editor) return { ok: false, error: 'WordPress editor verification data is unavailable.' };
        const actualTitle = String(editor.getEditedPostAttribute?.('title') || editor.getCurrentPostAttribute?.('title') || '').trim();
        const status = editor.getEditedPostAttribute?.('status') || editor.getCurrentPostAttribute?.('status') || '';
        let actualContent = String(editor.getEditedPostAttribute?.('content') || editor.getCurrentPostAttribute?.('content') || '');
        if (blocks?.getBlocks && wp?.blocks?.serialize) {
          const currentBlocks = blocks.getBlocks();
          if (Array.isArray(currentBlocks) && currentBlocks.length) actualContent = wp.blocks.serialize(currentBlocks);
        }
        const normalise = (value) => String(value || '')
          .replace(/<!--\s*\/?wp:[^>]*-->/g, ' ')
          .replace(/<[^>]+>/g, ' ')
          .replace(/&nbsp;/gi, ' ')
          .replace(/\s+/g, ' ')
          .trim();
        const requestedNormal = normalise(content);
        const actualNormal = normalise(actualContent);
        const contentMatch = actualNormal.includes(requestedNormal) || requestedNormal.includes(actualNormal);
        const statusMatch = requestedStatus === 'publish'
          ? status === 'publish'
          : ['draft', 'pending', 'private'].includes(status);
        const postId = editor.getCurrentPostId?.() ?? null;
        const postType = editor.getCurrentPostType?.() ?? null;
        const permalink = editor.getCurrentPostAttribute?.('link') || editor.getPermalink?.() || '';
        const modified = editor.getCurrentPostAttribute?.('modified') || editor.getCurrentPostAttribute?.('modified_gmt') || null;
        return {
          ok: Boolean(postId) && (!title || actualTitle === String(title).trim()) && contentMatch && statusMatch && !editor.isSavingPost?.(),
          title: actualTitle,
          status,
          postId,
          postType,
          permalink,
          modified,
          contentLength: actualContent.length,
          contentMatch,
          error: editor.getLastPostSaveError?.() ? JSON.stringify(editor.getLastPostSaveError()) : null
        };
      } catch (error) {
        return { ok: false, error: error instanceof Error ? error.message : String(error) };
      }
    }
  });
  return result?.result ?? { ok: false, error: 'WordPress verification returned no result.' };
}
