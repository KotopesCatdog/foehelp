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
      chrome.tabs.sendMessage(tab.id, { type: 'foe-overlay-toggle' }).catch(() => {});
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

// Обработка запроса на извлечение данных из MainParser.CityMapData
chrome.runtime.onMessage.addListener((msg, sender, sendResponse) => {
  if (msg.type !== 'foe-extract-citymap') return;

  const tabId = sender.tab && sender.tab.id;
  if (!tabId) {
    sendResponse({ error: 'Нет доступа к табу' });
    return;
  }

  chrome.scripting.executeScript({
    target: { tabId: tabId },
    world: 'MAIN',
    func: () => {
      try {
        if (typeof MainParser === 'undefined') {
          return { error: 'MainParser не найден. Убедитесь что FoE Helper загружен.' };
        }
        // CityBuildingsData содержит подробные данные производств (resources с subType)
        // CityMapData содержит координаты и базовые данные
        var cbd = MainParser.CityBuildingsData;
        var cmd = MainParser.CityMapData;
        if (!cbd && !cmd) {
          return { error: 'Данные города не найдены. Откройте город и дождитесь загрузки.' };
        }
        // Объединяем: берём координаты из CityMapData, производства из CityBuildingsData
        var merged = {};
        if (cmd) {
          Object.keys(cmd).forEach(function(k) {
            merged[k] = Object.assign({}, cmd[k]);
          });
        }
        if (cbd) {
          Object.keys(cbd).forEach(function(k) {
            if (merged[k]) {
              // Дополняем данными из CityBuildingsData (resources, productions и т.д.)
              Object.assign(merged[k], cbd[k]);
              // Сохраняем координаты из CityMapData если есть
              if (cmd && cmd[k] && cmd[k].coords) {
                merged[k].coords = cmd[k].coords;
              }
            } else {
              merged[k] = Object.assign({}, cbd[k]);
            }
          });
        }
        return { data: merged };
      } catch (e) {
        return { error: e.message };
      }
    }
  }).then(results => {
    if (results && results[0] && results[0].result) {
      sendResponse(results[0].result);
    } else {
      sendResponse({ error: 'Не удалось выполнить скрипт' });
    }
  }).catch(err => {
    sendResponse({ error: err.message });
  });

  // Возвращаем true чтобы sendResponse можно было вызвать асинхронно
  return true;
});
