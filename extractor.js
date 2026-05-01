// extractor.js — выполняется в контексте страницы (MAIN world)
// Читает MainParser.CityMapData и передаёт данные обратно в content script
// через window.postMessage.
(function() {
  try {
    var cityMap = window.MainParser && window.MainParser.CityMapData;
    if (!cityMap) {
      window.postMessage({
        type: 'foe-overlay-buildings',
        buildings: [],
        error: 'MainParser.CityMapData not found'
      }, '*');
      return;
    }
    var result = [];
    var keys = Object.keys(cityMap);
    for (var i = 0; i < keys.length; i++) {
      var b = cityMap[keys[i]];
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
    window.postMessage({
      type: 'foe-overlay-buildings',
      buildings: result
    }, '*');
  } catch(e) {
    window.postMessage({
      type: 'foe-overlay-buildings',
      buildings: [],
      error: e.message
    }, '*');
  }
})();
