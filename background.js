// background.js — service worker
// По клику на иконку: проверяем наличие overlay в табе,
// если нет — инжектируем файлы, если есть — toggle панели.
// Также обрабатываем запрос на извлечение данных из MainParser.

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

// Обработка запроса на извлечение данных зданий
// Content script не имеет доступа к MainParser (изолированный мир),
// поэтому background выполняет скрипт в MAIN world страницы.
chrome.runtime.onMessage.addListener((msg, sender, sendResponse) => {
  if (msg.type !== 'foe-extract-buildings') return;
  if (!sender.tab || !sender.tab.id) return;

  chrome.scripting.executeScript({
    target: { tabId: sender.tab.id },
    world: 'MAIN',
    func: () => {
      try {
        const cityMap = window.MainParser && window.MainParser.CityMapData;
        if (!cityMap) {
          return { buildings: [], error: 'MainParser.CityMapData не найден. Убедитесь что FoE Helper загружен.' };
        }
        const result = [];
        for (const key of Object.keys(cityMap)) {
          const b = cityMap[key];
          result.push({
            name: b.name || '',
            cityentity_id: b.cityentity_id || '',
            x: b.x != null ? b.x : 0,
            y: b.y != null ? b.y : 0,
            width: b.width || 1,
            height: b.height || 1,
            productions: b.productions || [],
            state: b.state || {}
          });
        }
        return { buildings: result };
      } catch (e) {
        return { buildings: [], error: e.message };
      }
    }
  }).then(results => {
    const data = results && results[0] && results[0].result;
    sendResponse(data || { buildings: [], error: 'Не удалось выполнить скрипт' });
  }).catch(err => {
    sendResponse({ buildings: [], error: err.message });
  });

  // true = будем отвечать асинхронно
  return true;
});
