// ============================================================
// FoE City Overlay — content script
// Рисует изометрическую сетку поверх игры.
// Координаты (0,0) = левый угол сетки (верхушка ромба).
// ============================================================

(function () {
  'use strict';

  // ── ДЕФОЛТНЫЕ НАСТРОЙКИ ──────────────────────────────────
  const DEFAULTS = {
    isoA:       20,   // сдвиг вдоль оси колонок (вправо-вниз)
    isoB:       20,   // сдвиг вдоль оси рядов   (влево-вниз)
    tileW:      58,   // ширина одной клетки (горизонталь)
    tileH:      29,   // высота одной клетки (вертикаль)
    cols:       50,   // количество колонок сетки
    rows:       50,   // количество рядов сетки
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

  // ── ИЗВЛЕЧЕНИЕ ДАННЫХ ЧЕРЕЗ ИНЖЕКТ В СТРАНИЦУ ────────────
  // Content script работает в изолированном мире и не имеет
  // доступа к window.MainParser. Инжектируем скрипт в контекст
  // страницы, который читает CityMapData и передаёт обратно
  // через postMessage.

  function extractBuildingData() {
    const script = document.createElement('script');
    script.textContent = `
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
    `;
    document.documentElement.appendChild(script);
    script.remove();
  }

  // Слушаем ответ от инжектированного скрипта
  window.addEventListener('message', function(event) {
    if (event.source !== window) return;
    if (!event.data || event.data.type !== 'foe-overlay-buildings') return;

    var buildings = event.data.buildings || [];

    if (event.data.error) {
      console.warn('FoE Overlay:', event.data.error);
      updateStatus('Ошибка: ' + event.data.error);
      return;
    }

    var found = [];
    buildings.forEach(function(b) {
      var name = b.name.toLowerCase();
      if (!TARGET_NAMES.has(name)) return;

      // Проверяем productions на resource === "money"
      var prods = b.productions;
      if (!Array.isArray(prods) || prods.length === 0) return;

      var hasMoney = prods.some(function(p) {
        return p.type === 'resources' && p.resource === 'money';
      });
      if (!hasMoney) return;

      found.push({
        x: b.x,
        y: b.y,
        w: b.width,
        h: b.height,
        name: b.name
      });
    });

    highlightedBuildings = found;
    draw();
    updateStatus('Найдено зданий: ' + found.length);
  });

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

      <button id="foe-ov-extract">🔍 Извлечь данные</button>
      <div id="foe-ov-status" class="foe-ov-status-text"></div>

      <div class="foe-ov-divider"></div>

      <div class="foe-ov-row">
        <span class="foe-ov-label">↘ Ось X</span>
        <input type="range" id="ov-isoA" min="0" max="150" step="1">
        <span class="foe-ov-val" id="val-isoA"></span>
      </div>

      <div class="foe-ov-row">
        <span class="foe-ov-label">↙ Ось Y</span>
        <input type="range" id="ov-isoB" min="-80" max="150" step="1">
        <span class="foe-ov-val" id="val-isoB"></span>
      </div>

      <div class="foe-ov-divider"></div>

      <div class="foe-ov-row">
        <span class="foe-ov-label">Клетка W</span>
        <input type="range" id="ov-tileW" min="20" max="160" step="1">
        <span class="foe-ov-val" id="val-tileW"></span>
      </div>

      <div class="foe-ov-row">
        <span class="foe-ov-label">Клетка H</span>
        <input type="range" id="ov-tileH" min="10" max="80" step="1">
        <span class="foe-ov-val" id="val-tileH"></span>
      </div>

      <div class="foe-ov-divider"></div>

      <div class="foe-ov-row">
        <span class="foe-ov-label">Колонки</span>
        <input type="range" id="ov-cols" min="5" max="120" step="1">
        <span class="foe-ov-val" id="val-cols"></span>
      </div>

      <div class="foe-ov-row">
        <span class="foe-ov-label">Ряды</span>
        <input type="range" id="ov-rows" min="5" max="120" step="1">
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

    // Суб-сетка (шаг 0.5)
    drawLines(0.5, 1, opacity * 0.3);

    // Основная сетка (шаг 1)
    drawLines(1, 2, opacity);

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

    // Номера осей через каждые 5 клеток
    ctx.globalAlpha = opacity * 0.7;
    ctx.fillStyle   = '#e8a020';
    ctx.font        = '10px "Share Tech Mono", monospace';

    for (let c = 0; c <= cols; c += 5) {
      const p = isoPoint(c, 0);
      ctx.textAlign = 'center';
      ctx.fillText('x' + c, p.x, p.y - 4);
    }
    for (let r = 0; r <= rows; r += 5) {
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
      chrome.storage.local.set({ foeOverlayCfg: cfg });
    } catch (e) {}
  }

  function loadAndInit() {
    try {
      chrome.storage.local.get('foeOverlayCfg', result => {
        if (result && result.foeOverlayCfg) {
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
