// ============================================================
// FoE City Overlay — content script
// Рисует изометрическую сетку поверх игры.
// Координаты (0,0) = левый угол сетки (верхушка ромба).
// ============================================================

(function () {
  'use strict';

  // Защита от повторной инжекции (service worker может перезапуститься)
  if (document.getElementById('foe-overlay-canvas')) return;

  // ── ДЕФОЛТНЫЕ НАСТРОЙКИ ──────────────────────────────────
  const DEFAULTS = {
    isoA:       40,   // сдвиг вдоль оси колонок (вправо-вниз)
    isoB:       40,   // сдвиг вдоль оси рядов   (влево-вниз)
    tileW:      29,   // ширина одной клетки (горизонталь)
    tileH:      15,   // высота одной клетки (вертикаль)
    cols:      100,   // количество колонок сетки
    rows:      100,   // количество рядов сетки
    opacity:    0.55, // прозрачность линий
    visible:    true  // показывать ли сетку
  };

  let cfg = { ...DEFAULTS };

  // ── ЦЕЛЕВЫЕ ЗДАНИЯ ───────────────────────────────────────
  // Все имена приведены к нижнему регистру для сравнения
  const TARGET_NAMES = new Set([
    'фонтан молодости',
    'маленький фонтан молодости',
    'колодец желаний',
    'маленький колодец желаний',
    // английские названия на случай другой локали
    'fountain of youth',
    'small fountain of youth',
    'wishing well',
    'small wishing well',
  ]);

  // Список зданий для подсветки: [{ x, y, w, h, name }]
  let highlightedBuildings = [];

  // ── ЗАГРУЗКА ДАННЫХ ИЗ JSON ФАЙЛА ─────────────────────────
  // Пользователь экспортирует данные из FoE Helper в JSON,
  // затем загружает файл через кнопку в панели.

  // Скрытый input для выбора файла
  const fileInput = document.createElement('input');
  fileInput.type = 'file';
  fileInput.accept = '.json';
  fileInput.style.display = 'none';
  document.body.appendChild(fileInput);

  fileInput.addEventListener('change', function() {
    const file = fileInput.files[0];
    if (!file) return;

    updateStatus('Чтение файла...');
    const reader = new FileReader();

    reader.onload = function(e) {
      try {
        const data = JSON.parse(e.target.result);
        processBuildingData(data);
      } catch (err) {
        console.warn('FoE Overlay: ошибка парсинга JSON:', err);
        updateStatus('Ошибка: невалидный JSON');
      }
    };

    reader.onerror = function() {
      updateStatus('Ошибка чтения файла');
    };

    reader.readAsText(file);
    // Сбрасываем input чтобы можно было выбрать тот же файл повторно
    fileInput.value = '';
  });

  function extractBuildingData() {
    fileInput.click();
  }

  // Рекурсивно ищем все объекты зданий (с полем "name") в JSON любой структуры
  function flattenBuildings(data) {
    var result = [];

    if (Array.isArray(data)) {
      data.forEach(function(item) {
        if (item && typeof item === 'object') {
          if (item.name && typeof item.name === 'string') {
            result.push(item);
          } else {
            result = result.concat(flattenBuildings(item));
          }
        }
      });
    } else if (data && typeof data === 'object') {
      // Если сам объект похож на здание — добавляем
      if (data.name && typeof data.name === 'string' && (data.id != null || data.entityId)) {
        result.push(data);
      } else {
        // Иначе ищем вглубь (byBuilding, values и т.д.)
        Object.values(data).forEach(function(val) {
          if (val && typeof val === 'object') {
            result = result.concat(flattenBuildings(val));
          }
        });
      }
    }

    return result;
  }

  function processBuildingData(data) {
    var buildings = flattenBuildings(data);

    console.log('[FoE Overlay] Найдено объектов зданий:', buildings.length);

    // Отладка: показываем первые 3 имени
    if (buildings.length > 0) {
      var names = buildings.slice(0, 5).map(function(b) { return b.name; });
      console.log('[FoE Overlay] Примеры зданий:', names);
    }

    var targetFound = [];
    var found = [];

    buildings.forEach(function(b) {
      if (!b || typeof b !== 'object' || !b.name) return;
      var name = b.name.toLowerCase();
      if (!TARGET_NAMES.has(name)) return;

      // Это целевое здание — логируем всю инфу о производстве
      targetFound.push(b.name);
      console.log('[FoE Overlay] Целевое здание:', b.name, 'id:', b.id);
      console.log('[FoE Overlay]   state:', JSON.stringify(b.state));
      console.log('[FoE Overlay]   productions:', JSON.stringify(b.productions));
      console.log('[FoE Overlay]   production:', JSON.stringify(b.production));
      console.log('[FoE Overlay]   coords:', JSON.stringify(b.coords));
      console.log('[FoE Overlay]   size:', JSON.stringify(b.size));

      // Проверяем производство на money в нескольких форматах
      var hasMoney = false;

      // Формат 1: state.production [{type:"resources", resources:{money:N}}]
      var stateProd = b.state && b.state.production;
      if (Array.isArray(stateProd)) {
        hasMoney = stateProd.some(function(p) {
          return p.type === 'resources' && p.resources && p.resources.money > 0;
        });
        console.log('[FoE Overlay]   state.production check:', hasMoney);
      }

      // Формат 2: productions [{type:"resources", resource:"money"}]
      if (!hasMoney && Array.isArray(b.productions)) {
        hasMoney = b.productions.some(function(p) {
          return p.type === 'resources' && p.resource === 'money';
        });
        console.log('[FoE Overlay]   productions check:', hasMoney);
      }

      // Формат 3: production (не массив state.production, а поле верхнего уровня)
      if (!hasMoney && Array.isArray(b.production)) {
        hasMoney = b.production.some(function(p) {
          if (p.type === 'resources' && p.resources && p.resources.money > 0) return true;
          if (p.type === 'resources' && p.resource === 'money') return true;
          return false;
        });
        console.log('[FoE Overlay]   production (top-level) check:', hasMoney);
      }

      console.log('[FoE Overlay]   hasMoney итого:', hasMoney);

      if (!hasMoney) return;

      // Координаты: coords.x/y или b.x/y
      var bx = (b.coords && b.coords.x != null) ? b.coords.x : (b.x != null ? b.x : 0);
      var by = (b.coords && b.coords.y != null) ? b.coords.y : (b.y != null ? b.y : 0);

      // Размер: size.width/length или b.width/b.height
      var bw = (b.size && b.size.width) || b.width || 1;
      var bh = (b.size && b.size.length) || b.height || 1;

      found.push({ x: bx, y: by, w: bw, h: bh, name: b.name });
    });

    console.log('[FoE Overlay] Целевых зданий найдено по имени:', targetFound.length, targetFound);
    console.log('[FoE Overlay] Из них с money:', found.length);

    highlightedBuildings = found;
    draw();
    updateStatus('Найдено зданий: ' + found.length + ' (целевых: ' + targetFound.length + ')');
  }

  // ── DOM ──────────────────────────────────────────────────
  const canvas = document.createElement('canvas');
  canvas.id = 'foe-overlay-canvas';
  document.body.appendChild(canvas);
  const ctx = canvas.getContext('2d');

  // Кнопка-открывалка (когда панель скрыта)
  const openBtn = document.createElement('button');
  openBtn.id = 'foe-overlay-open-btn';
  openBtn.textContent = '⬡ FoE Grid';
  document.body.appendChild(openBtn);

  // Панель управления
  const panel = document.createElement('div');
  panel.id = 'foe-overlay-panel';
  panel.innerHTML = `
    <div id="foe-overlay-header">
      <span id="foe-overlay-title">⬡ FoE Grid</span>
      <button id="foe-overlay-toggle-vis">сетка ВКЛ</button>
    </div>
    <div id="foe-overlay-body">

      <button id="foe-ov-extract">📂 Загрузить JSON</button>
      <div id="foe-ov-status" class="foe-ov-status-text"></div>

      <div class="foe-ov-divider"></div>

      <div class="foe-ov-row">
        <span class="foe-ov-label">↘ Ось X</span>
        <input type="range" id="ov-isoA" min="0" max="300" step="1">
        <span class="foe-ov-val" id="val-isoA"></span>
      </div>

      <div class="foe-ov-row">
        <span class="foe-ov-label">↙ Ось Y</span>
        <input type="range" id="ov-isoB" min="-80" max="300" step="1">
        <span class="foe-ov-val" id="val-isoB"></span>
      </div>

      <div class="foe-ov-divider"></div>

      <div class="foe-ov-row">
        <span class="foe-ov-label">Клетка W</span>
        <input type="range" id="ov-tileW" min="5" max="160" step="1">
        <span class="foe-ov-val" id="val-tileW"></span>
      </div>

      <div class="foe-ov-row">
        <span class="foe-ov-label">Клетка H</span>
        <input type="range" id="ov-tileH" min="3" max="80" step="1">
        <span class="foe-ov-val" id="val-tileH"></span>
      </div>

      <div class="foe-ov-divider"></div>

      <div class="foe-ov-row">
        <span class="foe-ov-label">Колонки</span>
        <input type="range" id="ov-cols" min="5" max="200" step="1">
        <span class="foe-ov-val" id="val-cols"></span>
      </div>

      <div class="foe-ov-row">
        <span class="foe-ov-label">Ряды</span>
        <input type="range" id="ov-rows" min="5" max="200" step="1">
        <span class="foe-ov-val" id="val-rows"></span>
      </div>

      <div class="foe-ov-divider"></div>

      <div class="foe-ov-row">
        <span class="foe-ov-label">Прозр.</span>
        <input type="range" id="ov-opacity" min="0.05" max="1" step="0.05">
        <span class="foe-ov-val" id="val-opacity"></span>
      </div>

      <div class="foe-ov-hint">
        Тащи панель за заголовок.<br>
        Нажми «сетка» чтобы скрыть линии.<br>
        Кнопка ✕ прячет панель.
      </div>

      <div class="foe-ov-divider"></div>

      <button id="foe-ov-reset">↺ Сброс</button>

    </div>

    <div style="display:flex;justify-content:flex-end;padding:4px 8px 6px;">
      <button id="foe-ov-close" style="background:none;border:none;color:#6c7a99;font-size:12px;cursor:pointer;font-family:inherit;letter-spacing:.5px;">✕ скрыть панель</button>
    </div>
  `;
  document.body.appendChild(panel);

  // ── КНОПКА «ИЗВЛЕЧЬ ДАННЫЕ» ──────────────────────────────
  document.getElementById('foe-ov-extract').addEventListener('click', function() {
    updateStatus('Извлечение данных...');
    extractBuildingData();
  });

  function updateStatus(text) {
    var el = document.getElementById('foe-ov-status');
    if (el) el.textContent = text;
  }

  // ── СЛАЙДЕРЫ ─────────────────────────────────────────────
  const sliders = [
    { id: 'isoA', label: 'val-isoA' },
    { id: 'isoB', label: 'val-isoB' },
    { id: 'tileW',   label: 'val-tileW'   },
    { id: 'tileH',   label: 'val-tileH'   },
    { id: 'cols',    label: 'val-cols'     },
    { id: 'rows',    label: 'val-rows'     },
    { id: 'opacity', label: 'val-opacity'  },
  ];

  function syncSliders() {
    sliders.forEach(({ id, label }) => {
      const el = document.getElementById('ov-' + id);
      const vl = document.getElementById(label);
      if (!el || !vl) return;
      el.value = cfg[id];
      vl.textContent = id === 'opacity'
        ? Math.round(cfg[id] * 100) + '%'
        : cfg[id];
    });
  }

  sliders.forEach(({ id, label }) => {
    const el = document.getElementById('ov-' + id);
    const vl = document.getElementById(label);
    if (!el) return;
    el.addEventListener('input', () => {
      cfg[id] = parseFloat(el.value);
      vl.textContent = id === 'opacity'
        ? Math.round(cfg[id] * 100) + '%'
        : cfg[id];
      draw();
      save();
    });

    // Точная настройка колесом мыши
    el.addEventListener('wheel', e => {
      e.preventDefault();
      const step = id === 'opacity' ? 0.005
           : (id === 'isoA' || id === 'isoB') ? 0.1
           : 0.5;
      const dir  = e.deltaY < 0 ? 1 : -1;
      const min  = parseFloat(el.min);
      const max  = parseFloat(el.max);
      cfg[id] = Math.min(max, Math.max(min, Math.round((cfg[id] + dir * step) * 100) / 100));
      el.value = cfg[id];
      vl.textContent = id === 'opacity'
        ? Math.round(cfg[id] * 100) + '%'
        : cfg[id];
      draw();
      save();
    }, { passive: false });
  });

  // ── ВИДИМОСТЬ СЕТКИ ──────────────────────────────────────
  const toggleVis = document.getElementById('foe-overlay-toggle-vis');
  toggleVis.addEventListener('click', () => {
    cfg.visible = !cfg.visible;
    toggleVis.textContent = cfg.visible ? 'сетка ВКЛ' : 'сетка ВЫКЛ';
    toggleVis.classList.toggle('off', !cfg.visible);
    draw();
    save();
  });

  // ── СКРЫТЬ / ОТКРЫТЬ ПАНЕЛЬ ──────────────────────────────
  document.getElementById('foe-ov-close').addEventListener('click', () => {
    panel.classList.add('hidden');
    openBtn.style.display = 'block';
  });
  openBtn.addEventListener('click', () => {
    panel.classList.remove('hidden');
    openBtn.style.display = 'none';
  });

  // ── СБРОС ────────────────────────────────────────────────
  document.getElementById('foe-ov-reset').addEventListener('click', () => {
    cfg = { ...DEFAULTS };
    highlightedBuildings = [];
    updateStatus('');
    syncSliders();
    draw();
    save();
  });

  // ── ПЕРЕТАСКИВАНИЕ ПАНЕЛИ ────────────────────────────────
  (function makeDraggable() {
    const header = document.getElementById('foe-overlay-header');
    let dragging = false, startX, startY, origLeft, origTop;

    header.addEventListener('mousedown', e => {
      if (e.target.tagName === 'BUTTON') return;
      dragging = true;
      startX = e.clientX;
      startY = e.clientY;
      const rect = panel.getBoundingClientRect();
      origLeft = rect.left;
      origTop  = rect.top;
      panel.style.right = 'auto';
      panel.style.left  = origLeft + 'px';
      panel.style.top   = origTop  + 'px';
      e.preventDefault();
    });

    document.addEventListener('mousemove', e => {
      if (!dragging) return;
      const dx = e.clientX - startX;
      const dy = e.clientY - startY;
      panel.style.left = Math.max(0, origLeft + dx) + 'px';
      panel.style.top  = Math.max(0, origTop  + dy) + 'px';
    });

    document.addEventListener('mouseup', () => { dragging = false; });
  })();

  // ── CANVAS RESIZE ────────────────────────────────────────
  function resizeCanvas() {
    canvas.width  = window.innerWidth;
    canvas.height = window.innerHeight;
    draw();
  }
  window.addEventListener('resize', resizeCanvas);

  // ── РИСОВАНИЕ СЕТКИ ──────────────────────────────────────
  // Изометрия FoE:
  //   Ось A (колонки): вектор ( tileW/2,  tileH/2)
  //   Ось B (ряды):    вектор (-tileW/2,  tileH/2)

  function isoPoint(col, row) {
    const hw = cfg.tileW / 2;
    const hh = cfg.tileH / 2;
    const originX = cfg.isoA * hw - cfg.isoB * hw;
    const originY = cfg.isoA * hh + cfg.isoB * hh;
    return {
      x: originX + (col - row) * hw,
      y: originY + (col + row) * hh
    };
  }

  function draw() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    if (!cfg.visible) return;

    const { cols, rows, opacity } = cfg;

    ctx.save();

    function snap1(v) { return Math.round(v - 0.5) + 0.5; }
    function snap2(v) { return Math.round(v); }

    function drawLines(step, lw, alpha) {
      ctx.globalAlpha = alpha;
      ctx.strokeStyle = '#e8a020';
      ctx.lineWidth   = lw;
      const snapFn = (lw % 2 === 1) ? snap1 : snap2;

      const totalR = Math.round(rows / step);
      for (let ri = 0; ri <= totalR; ri++) {
        const r = ri * step;
        const start = isoPoint(0,    r);
        const end   = isoPoint(cols, r);
        ctx.beginPath();
        ctx.moveTo(snapFn(start.x), snapFn(start.y));
        ctx.lineTo(snapFn(end.x),   snapFn(end.y));
        ctx.stroke();
      }

      const totalC = Math.round(cols / step);
      for (let ci = 0; ci <= totalC; ci++) {
        const c = ci * step;
        const start = isoPoint(c, 0);
        const end   = isoPoint(c, rows);
        ctx.beginPath();
        ctx.moveTo(snapFn(start.x), snapFn(start.y));
        ctx.lineTo(snapFn(end.x),   snapFn(end.y));
        ctx.stroke();
      }
    }

    // Мелкая сетка (каждая клетка = 1 тайл игры)
    drawLines(1, 1, opacity * 0.3);

    // Крупная сетка (каждые 4 клетки — жирные линии)
    drawLines(4, 2, opacity);

    // Начало координат (0,0) — зелёный ромб
    const o0 = isoPoint(0, 0);
    const o1 = isoPoint(1, 0);
    const o2 = isoPoint(1, 1);
    const o3 = isoPoint(0, 1);

    ctx.strokeStyle = '#a6e3a1';
    ctx.lineWidth   = 2.5;
    ctx.beginPath();
    ctx.moveTo(o0.x, o0.y);
    ctx.lineTo(o1.x, o1.y);
    ctx.lineTo(o2.x, o2.y);
    ctx.lineTo(o3.x, o3.y);
    ctx.closePath();
    ctx.stroke();

    // Метка "0,0"
    ctx.globalAlpha = Math.min(opacity * 1.5, 1);
    ctx.fillStyle   = '#a6e3a1';
    ctx.font        = 'bold 11px "Share Tech Mono", monospace';
    ctx.textAlign   = 'center';
    ctx.fillText('0,0', o0.x, o0.y - 5);

    // Номера осей через каждые 10 клеток
    ctx.globalAlpha = opacity * 0.7;
    ctx.fillStyle   = '#e8a020';
    ctx.font        = '10px "Share Tech Mono", monospace';

    for (let c = 0; c <= cols; c += 10) {
      const p = isoPoint(c, 0);
      ctx.textAlign = 'center';
      ctx.fillText('x' + c, p.x, p.y - 4);
    }
    for (let r = 0; r <= rows; r += 10) {
      const p = isoPoint(0, r);
      ctx.textAlign = 'right';
      ctx.fillText('y' + r, p.x - 4, p.y + 3);
    }

    // ── Подсветка зданий с money ───────────────────────────
    if (highlightedBuildings.length > 0) {
      highlightedBuildings.forEach(b => {
        const top   = isoPoint(b.x,       b.y      );
        const right = isoPoint(b.x + b.w, b.y      );
        const bot   = isoPoint(b.x + b.w, b.y + b.h);
        const left  = isoPoint(b.x,       b.y + b.h);

        // Заливка — золотая
        ctx.globalAlpha = 0.35;
        ctx.fillStyle   = '#f9e2af';
        ctx.beginPath();
        ctx.moveTo(top.x,   top.y);
        ctx.lineTo(right.x, right.y);
        ctx.lineTo(bot.x,   bot.y);
        ctx.lineTo(left.x,  left.y);
        ctx.closePath();
        ctx.fill();

        // Обводка
        ctx.globalAlpha = 0.9;
        ctx.strokeStyle = '#f9e2af';
        ctx.lineWidth   = 2;
        ctx.beginPath();
        ctx.moveTo(top.x,   top.y);
        ctx.lineTo(right.x, right.y);
        ctx.lineTo(bot.x,   bot.y);
        ctx.lineTo(left.x,  left.y);
        ctx.closePath();
        ctx.stroke();

        // Иконка монеты в центре здания
        const cx = (top.x + bot.x) / 2;
        const cy = (top.y + bot.y) / 2;
        ctx.globalAlpha = 1;
        ctx.font        = `${Math.max(12, cfg.tileH * b.h * 0.5)}px serif`;
        ctx.textAlign   = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText('🪙', cx, cy);
      });
    }

    ctx.restore();
  }

  // ── ХРАНЕНИЕ НАСТРОЕК ────────────────────────────────────
  function save() {
    try {
      chrome.storage.local.set({ foeOverlayCfg: { ...cfg, _v: CFG_VERSION } });
    } catch (e) {}
  }

  const CFG_VERSION = 2; // увеличить при изменении дефолтов

  function loadAndInit() {
    try {
      chrome.storage.local.get('foeOverlayCfg', result => {
        if (result && result.foeOverlayCfg && result.foeOverlayCfg._v === CFG_VERSION) {
          cfg = { ...DEFAULTS, ...result.foeOverlayCfg };
        }
        toggleVis.textContent = cfg.visible ? 'сетка ВКЛ' : 'сетка ВЫКЛ';
        toggleVis.classList.toggle('off', !cfg.visible);
        syncSliders();
        resizeCanvas();
      });
    } catch (e) {
      syncSliders();
      resizeCanvas();
    }
  }

  // ── TOGGLE по сообщению от background ────────────────────
  chrome.runtime.onMessage.addListener((msg) => {
    if (msg.type !== 'foe-overlay-toggle') return;
    const hidden = panel.classList.contains('hidden');
    if (hidden) {
      panel.classList.remove('hidden');
      openBtn.style.display = 'none';
    } else {
      panel.classList.add('hidden');
      openBtn.style.display = 'block';
    }
  });

  // ── СТАРТ ────────────────────────────────────────────────
  loadAndInit();

})();
