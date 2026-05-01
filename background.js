// background.js — service worker
// По клику на иконку: если overlay ещё не загружен — инжектируем файлы,
// затем отправляем toggle-сообщение (показать/скрыть панель).

const FOE_INJECTED = new Set(); // tab ids где уже заинжектировано

chrome.action.onClicked.addListener(async (tab) => {
  if (!tab.id) return;

  // Проверяем что это страница FoE
  if (!tab.url || !tab.url.includes('forgeofempires.com')) return;

  try {
    if (!FOE_INJECTED.has(tab.id)) {
      // Первый клик — инжектируем CSS и JS
      await chrome.scripting.insertCSS({
        target: { tabId: tab.id },
        files:  ['overlay.css']
      });
      await chrome.scripting.executeScript({
        target: { tabId: tab.id },
        files:  ['content.js']
      });
      FOE_INJECTED.add(tab.id);
    } else {
      // Повторный клик — toggle панели
      chrome.tabs.sendMessage(tab.id, { type: 'foe-overlay-toggle' });
    }
  } catch (e) {
    console.error('FoE Overlay inject error:', e);
  }
});

// Если вкладка закрыта или обновлена — сбрасываем флаг инжекции
chrome.tabs.onRemoved.addListener(tabId => FOE_INJECTED.delete(tabId));
chrome.tabs.onUpdated.addListener((tabId, info) => {
  if (info.status === 'loading') FOE_INJECTED.delete(tabId);
});
