// background.js — service worker
// По клику на иконку: проверяем наличие overlay в табе,
// если нет — инжектируем файлы, если есть — toggle панели.

chrome.action.onClicked.addListener(async (tab) => {
  if (!tab.id) return;

  // Проверяем что это страница FoE
  if (!tab.url || !tab.url.includes('forgeofempires.com')) return;

  try {
    // Проверяем, инжектирован ли уже content script
    const [result] = await chrome.scripting.executeScript({
      target: { tabId: tab.id },
      func: () => !!document.getElementById('foe-overlay-canvas')
    });

    if (result && result.result) {
      // Overlay уже есть — toggle панели
      chrome.tabs.sendMessage(tab.id, { type: 'foe-overlay-toggle' });
    } else {
      // Первый раз — инжектируем CSS и JS
      await chrome.scripting.insertCSS({
        target: { tabId: tab.id },
        files:  ['overlay.css']
      });
      await chrome.scripting.executeScript({
        target: { tabId: tab.id },
        files:  ['content.js']
      });
    }
  } catch (e) {
    console.error('FoE Overlay inject error:', e);
  }
});
