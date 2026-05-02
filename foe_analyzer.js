// =========================================================
// ================  ГЛОБАЛЬНЫЕ ПЕРЕМЕННЫЕ ==================
// =========================================================

let perTileMode = false;
let inventoryMode = 0;   // 0=все, 1=только инвентарь, 2=без инвентаря
let lastList = [];
let lastGrouped = [];

let favorites = JSON.parse(localStorage.getItem("favorites") || "[]");


// =========================================================
// ===================== DOM READY ==========================
// =========================================================

document.addEventListener("DOMContentLoaded", () => {

    const uploadArea = document.getElementById('uploadArea');
    const fileInput = document.getElementById('fileInput');

    uploadArea.onclick = () => fileInput.click();
    fileInput.onchange = () => loadFile(fileInput.files[0]);

    // --- вкладки ---
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.onclick = () => {
            document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
            document.querySelectorAll('.tab-content').forEach(t => t.classList.remove('active'));
            btn.classList.add('active');
            document.getElementById('tab-' + btn.dataset.tab).classList.add('active');
        };
    });

    // --- поиск по имени ---
    document.getElementById("searchInput")?.addEventListener("input", function() {
        document.querySelectorAll("tbody tr").forEach(tr => syncRowVisibility(tr));
    });

    // --- переключатель "на клетку" ---
    document.getElementById("togglePerTile").addEventListener("click", function() {
        perTileMode = !perTileMode;
        this.textContent = perTileMode ? "На клетку: вкл" : "На клетку: выкл";
        this.classList.toggle("active", perTileMode);
        rebuildAllTables();
    });

    // --- переключатель "инвентарь" ---
    document.getElementById("toggleInventory").addEventListener("click", function() {
        inventoryMode = (inventoryMode + 1) % 3;
        const labels = ["Инвентарь: все", "Инвентарь: только", "Инвентарь: нет"];
        this.textContent = labels[inventoryMode];
        this.classList.toggle("active", inventoryMode !== 0);
        applyInventoryFilter();
    });
});


// =========================================================
// ===================== FAVORITES ==========================
// =========================================================

function toggleFavorite(entityId) {
    const i = favorites.indexOf(entityId);
    if (i >= 0) favorites.splice(i, 1);
    else favorites.push(entityId);
    localStorage.setItem("favorites", JSON.stringify(favorites));
}

function updateFavoriteVisual(entityId) {
    document.querySelectorAll(`.fav[data-id="${entityId}"]`).forEach(cell => {
        const isFav = favorites.includes(entityId);
        cell.textContent = isFav ? "★" : "☆";
        cell.classList.toggle("active", isFav);
        const tr = cell.closest("tr");
        if (tr) tr.classList.toggle("favorite-row", isFav);
    });
}


// =========================================================
// ======================= ЗАГРУЗКА =========================
// =========================================================

function loadFile(file) {
    const reader = new FileReader();
    reader.onload = () => {
        let json;
        try { json = JSON.parse(reader.result); }
        catch(e) { alert("Ошибка JSON"); return; }

        if (!json.buildings || !Array.isArray(json.buildings)) {
            alert("Нет массива buildings");
            return;
        }
        loadBuildings(json.buildings);
    };
    reader.readAsText(file);
}

function loadBuildings(list) {
    lastList = list;
    const grouped = groupBuildings(list);
    lastGrouped = grouped;
    rebuildAllTables();
}

function rebuildAllTables() {
    fillAllBuildings(lastGrouped);
    fillFPTable(lastGrouped);
    fillGoodsTable(lastGrouped);
    fillClanGoodsTable(lastGrouped);
    fillSpecialGoodsTable(lastGrouped);
    fillMoneyTable(lastGrouped);
    fillSuppliesTable(lastGrouped);
    fillMedalsTable(lastGrouped);
    fillPremiumTable(lastGrouped);
    fillUnitsTable(lastGrouped);
    fillItemsTable(lastGrouped);
    fillPopulationTable(lastGrouped);
    fillHappinessTable(lastGrouped);
    fillBoostsTable(lastGrouped);
    fillGuildRaidsTable(lastGrouped);

    // включаем сортировку на всех таблицах
    document.querySelectorAll('table').forEach(t => enableSorting(t.id));
    applyInventoryFilter();
}


// =========================================================
// ===================== GROUPING ===========================
// =========================================================

function groupBuildings(list) {
    const map = new Map();
    list.forEach(b => {
        const key = b.name + "||" + b.eraName;
        if (!map.has(key)) {
            map.set(key, { ...b, count: 1 });
        } else {
            map.get(key).count++;
        }
    });
    return [...map.values()];
}


// =========================================================
// ================ EXTRACTION: ALL PRODUCTIONS =============
// =========================================================

/**
 * Извлекает ВСЕ возможные производства здания (не только текущие).
 * Для случайных (random) — считаем ожидаемое значение в сутки:
 *   amount * dropChance (= ожидание за 1 сбор при 24ч цикле)
 */
function extractAllProductions(b) {
    let fp = 0, fpRandom = 0;
    let prev = 0, age = 0, next = 0, goodsRandom = 0;
    let clanGoods = 0;
    let specialGoods = 0;
    let money = 0, moneyRandom = 0;
    let supplies = 0, suppliesRandom = 0;
    let medals = 0, medalsRandom = 0;
    let premium = 0, premiumRandom = 0;
    let units = 0, unitsRandom = 0;
    let items = []; // {name, amount, dropChance, daily}

    // Бусты
    let fpBoost = 0, coinBoost = 0, supplyBoost = 0, goodsBoost = 0;

    // Guild Raids
    let gr_actions = 0, gr_coins_pct = 0, gr_supplies_pct = 0;
    let gr_coins_start = 0, gr_supplies_start = 0, gr_goods_start = 0, gr_units_start = 0;

    const productions = b.production || [];

    for (const p of productions) {
        if (!p) continue;

        // === resources (прямые ресурсы) ===
        if (p.type === "resources" && p.resources) {
            for (const [k, v] of Object.entries(p.resources)) {
                if (k === "strategy_points") fp += v;
                else if (k === "money") money += v;
                else if (k === "supplies") supplies += v;
                else if (k === "medals") medals += v;
                else if (k === "premium") premium += v;
                else if (k.includes("good") && k.includes("previous")) prev += v;
                else if (k.includes("good") && (k.includes("all") || k.includes("age"))) age += v;
                else if (k.includes("good") && k.includes("next")) next += v;
            }
        }

        // === guildResources ===
        if (p.type === "guildResources" && p.resources) {
            const amount = p.resources.all_goods_of_age || Object.values(p.resources)[0] || 0;
            clanGoods += amount;
        }

        // === special_goods ===
        if (p.type === "special_goods" && p.resources) {
            for (const val of Object.values(p.resources)) {
                if (typeof val === "number") specialGoods += val;
            }
        }

        // === unit ===
        if (p.type === "unit" && p.resources) {
            for (const val of Object.values(p.resources)) {
                if (typeof val === "number") units += val;
            }
        }

        // === genericReward ===
        if (p.type === "genericReward" && p.resources) {
            const r = p.resources;
            if (r.type === "strategy_points") fp += r.amount || 0;
            else if (r.type === "goods" || r.type === "good") {
                if (r.subType && r.subType.includes("previous")) prev += r.amount || 0;
                else if (r.subType && (r.subType.includes("age") || r.subType.includes("all"))) age += r.amount || 0;
                else if (r.subType && r.subType.includes("next")) next += r.amount || 0;
            }
            else if (r.type === "consumable" || r.subType === "fragment") {
                items.push({
                    name: r.name || r.subType || "предмет",
                    amount: r.amount || 1,
                    dropChance: 1,
                    daily: r.amount || 1
                });
            }
        }

        // === random (шанс) ===
        if (p.type === "random" && Array.isArray(p.resources)) {
            for (const r of p.resources) {
                if (!r) continue;
                const amt = r.amount || 0;
                const chance = r.dropChance || 0;
                const daily = amt * chance; // ожидаемое в сутки

                if (r.subType === "random_good_of_previous_age" || (r.type === "good" && r.subType?.includes("previous"))) {
                    goodsRandom += daily;
                    prev += daily;
                } else if (r.subType === "random_good_of_age" || (r.type === "good" && r.subType?.includes("age"))) {
                    goodsRandom += daily;
                    age += daily;
                } else if (r.subType === "random_good_of_next_age" || (r.type === "good" && r.subType?.includes("next"))) {
                    goodsRandom += daily;
                    next += daily;
                } else if (r.type === "resources" && r.subType === "strategy_points") {
                    fpRandom += daily;
                    fp += daily;
                } else if (r.type === "resources" && r.subType === "money") {
                    moneyRandom += daily;
                    money += daily;
                } else if (r.type === "resources" && r.subType === "supplies") {
                    suppliesRandom += daily;
                    supplies += daily;
                } else if (r.type === "resources" && r.subType === "medals") {
                    medalsRandom += daily;
                    medals += daily;
                } else if (r.type === "resources" && r.subType === "premium") {
                    premiumRandom += daily;
                    premium += daily;
                } else if (r.type === "unit") {
                    unitsRandom += daily;
                    units += daily;
                } else if (r.type === "forgepoint_package" || r.subType === "forgepoint_package") {
                    fpRandom += daily;
                    fp += daily;
                } else if (r.type === "consumable" || r.type?.includes("chest")) {
                    items.push({
                        name: r.name || r.subType || r.type,
                        amount: amt,
                        dropChance: chance,
                        daily: daily
                    });
                } else if (r.type?.includes("good") && r.type?.includes("guild")) {
                    clanGoods += daily;
                } else if (r.type?.includes("good") && !r.type?.includes("guild")) {
                    goodsRandom += daily;
                    age += daily;
                }
            }
        }
    }

    // === Бусты из boosts массива ===
    if (b.boosts && Array.isArray(b.boosts)) {
        for (const boost of b.boosts) {
            if (!boost || !boost.type) continue;
            const types = Array.isArray(boost.type) ? boost.type : [boost.type];
            const v = boost.value || 0;
            const feature = boost.feature || "all";

            for (const t of types) {
                if (t === "forge_points_production") fpBoost += v;
                else if (t === "coin_production") coinBoost += v;
                else if (t === "supply_production") supplyBoost += v;
                else if (t === "goods_production") goodsBoost += v;

                // Guild Raids
                else if (t === "guild_raids_action_points_collection") gr_actions += v;
                else if (t === "guild_raids_coins_production") gr_coins_pct += v;
                else if (t === "guild_raids_supplies_production") gr_supplies_pct += v;
                else if (t === "guild_raids_coins_start") gr_coins_start += v;
                else if (t === "guild_raids_supplies_start") gr_supplies_start += v;
                else if (t === "guild_raids_goods_start") gr_goods_start += v;
                else if (t === "guild_raids_units_start") gr_units_start += v;
            }
        }
    }

    return {
        fp, fpRandom,
        prev, age, next, goodsRandom,
        clanGoods, specialGoods,
        money, moneyRandom,
        supplies, suppliesRandom,
        medals, medalsRandom,
        premium, premiumRandom,
        units, unitsRandom,
        items,
        fpBoost, coinBoost, supplyBoost, goodsBoost,
        gr_actions, gr_coins_pct, gr_supplies_pct,
        gr_coins_start, gr_supplies_start, gr_goods_start, gr_units_start
    };
}


// =========================================================
// =================== VALUE PER TILE =======================
// =========================================================

function calcCoef(b) {
    if (!b.size) return 1;
    const w = b.size.width;
    const h = b.size.length;
    const area = w * h;
    if (!b.needsStreet) return area;
    return area + Math.min(w, h) / 2;
}

function perTile(b, value) {
    if (!perTileMode) return value;
    const coef = calcCoef(b);
    return coef ? +(value / coef).toFixed(2) : value;
}

function nz(v) { return v ? (Number.isInteger(v) ? v : +v.toFixed(2)) : ''; }
function fmtSize(b) { return b.size ? (b.size.width + "×" + b.size.length) : ""; }
function isInv(b) { return (b.id === 0) ? "✔" : ""; }


// =========================================================
// =============== HELPER: CREATE ROW =======================
// =========================================================

function createBaseRow(b, extraCells) {
    const isFav = favorites.includes(b.entityId);
    const fav = isFav ? "★" : "☆";
    const tr = document.createElement("tr");
    tr.dataset.name = (b.name || "").toLowerCase();
    if (isFav) tr.classList.add("favorite-row");

    tr.innerHTML = `
        <td class="fav ${isFav ? "active" : ""}" data-id="${b.entityId}">${fav}</td>
        <td>${isInv(b)}</td>
        <td>${b.name}</td>
        <td>${b.count}</td>
        <td>${fmtSize(b)}</td>
        <td>${b.eraName || ''}</td>
        ${extraCells}
    `;

    tr.querySelector(".fav").onclick = (e) => {
        e.stopPropagation();
        toggleFavorite(b.entityId);
        updateFavoriteVisual(b.entityId);
    };

    return tr;
}


// =========================================================
// ===================== TABLE: ALL =========================
// =========================================================

function fillAllBuildings(list) {
    const tb = document.querySelector('#buildingsTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        const prod = extractAllProductions(b);
        const tr = createBaseRow(b, `
            <td>${nz(perTile(b, b.population || 0))}</td>
            <td>${nz(perTile(b, prod.fp))}</td>
            <td>${nz(perTile(b, prod.prev))}</td>
            <td>${nz(perTile(b, prod.age))}</td>
            <td>${nz(perTile(b, prod.next))}</td>
            <td>${nz(perTile(b, prod.clanGoods))}</td>
            <td>${nz(perTile(b, prod.specialGoods))}</td>
            <td>${nz(perTile(b, prod.money))}</td>
            <td>${nz(perTile(b, prod.supplies))}</td>
            <td>${nz(perTile(b, prod.medals))}</td>
            <td>${nz(perTile(b, prod.units))}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// ===================== TABLE: FP ==========================
// =========================================================

function fillFPTable(list) {
    const tb = document.querySelector('#fpTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        const prod = extractAllProductions(b);
        if (prod.fp <= 0) return;
        const tr = createBaseRow(b, `
            <td>${nz(perTile(b, prod.fp))}</td>
            <td>${nz(perTile(b, prod.fpRandom))}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// ===================== TABLE: GOODS =======================
// =========================================================

function fillGoodsTable(list) {
    const tb = document.querySelector('#goodsTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        const prod = extractAllProductions(b);
        if (prod.prev <= 0 && prod.age <= 0 && prod.next <= 0) return;
        const tr = createBaseRow(b, `
            <td>${nz(perTile(b, prod.prev))}</td>
            <td>${nz(perTile(b, prod.age))}</td>
            <td>${nz(perTile(b, prod.next))}</td>
            <td>${nz(perTile(b, prod.goodsRandom))}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// ================ TABLE: CLAN GOODS =======================
// =========================================================

function fillClanGoodsTable(list) {
    const tb = document.querySelector('#clanGoodsTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        const prod = extractAllProductions(b);
        if (prod.clanGoods <= 0) return;
        const tr = createBaseRow(b, `
            <td>${nz(perTile(b, prod.clanGoods))}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// ============== TABLE: SPECIAL GOODS ======================
// =========================================================

function fillSpecialGoodsTable(list) {
    const tb = document.querySelector('#specialGoodsTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        const prod = extractAllProductions(b);
        if (prod.specialGoods <= 0) return;
        const tr = createBaseRow(b, `
            <td>${nz(perTile(b, prod.specialGoods))}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// ===================== TABLE: MONEY =======================
// =========================================================

function fillMoneyTable(list) {
    const tb = document.querySelector('#moneyTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        const prod = extractAllProductions(b);
        if (prod.money <= 0) return;
        const tr = createBaseRow(b, `
            <td>${nz(perTile(b, prod.money))}</td>
            <td>${nz(perTile(b, prod.moneyRandom))}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// =================== TABLE: SUPPLIES ======================
// =========================================================

function fillSuppliesTable(list) {
    const tb = document.querySelector('#suppliesTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        const prod = extractAllProductions(b);
        if (prod.supplies <= 0) return;
        const tr = createBaseRow(b, `
            <td>${nz(perTile(b, prod.supplies))}</td>
            <td>${nz(perTile(b, prod.suppliesRandom))}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// =================== TABLE: MEDALS ========================
// =========================================================

function fillMedalsTable(list) {
    const tb = document.querySelector('#medalsTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        const prod = extractAllProductions(b);
        if (prod.medals <= 0) return;
        const tr = createBaseRow(b, `
            <td>${nz(perTile(b, prod.medals))}</td>
            <td>${nz(perTile(b, prod.medalsRandom))}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// ================== TABLE: PREMIUM ========================
// =========================================================

function fillPremiumTable(list) {
    const tb = document.querySelector('#premiumTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        const prod = extractAllProductions(b);
        if (prod.premium <= 0) return;
        const tr = createBaseRow(b, `
            <td>${nz(perTile(b, prod.premium))}</td>
            <td>${nz(perTile(b, prod.premiumRandom))}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// ==================== TABLE: UNITS ========================
// =========================================================

function fillUnitsTable(list) {
    const tb = document.querySelector('#unitsTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        const prod = extractAllProductions(b);
        if (prod.units <= 0) return;
        const tr = createBaseRow(b, `
            <td>${nz(perTile(b, prod.units))}</td>
            <td>${nz(perTile(b, prod.unitsRandom))}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// ==================== TABLE: ITEMS ========================
// =========================================================

function fillItemsTable(list) {
    const tb = document.querySelector('#itemsTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        const prod = extractAllProductions(b);
        if (prod.items.length === 0) return;

        for (const item of prod.items) {
            const isFav = favorites.includes(b.entityId);
            const fav = isFav ? "★" : "☆";
            const tr = document.createElement("tr");
            tr.dataset.name = (b.name || "").toLowerCase();
            if (isFav) tr.classList.add("favorite-row");

            tr.innerHTML = `
                <td class="fav ${isFav ? "active" : ""}" data-id="${b.entityId}">${fav}</td>
                <td>${isInv(b)}</td>
                <td>${b.name}</td>
                <td>${b.count}</td>
                <td>${fmtSize(b)}</td>
                <td>${b.eraName || ''}</td>
                <td>${item.name}</td>
                <td>${item.amount}</td>
                <td>${item.dropChance < 1 ? nz(item.daily) + ' (' + Math.round(item.dropChance * 100) + '%)' : item.amount}</td>
            `;

            tr.querySelector(".fav").onclick = (e) => {
                e.stopPropagation();
                toggleFavorite(b.entityId);
                updateFavoriteVisual(b.entityId);
            };
            frag.appendChild(tr);
        }
    });
    tb.appendChild(frag);
}


// =========================================================
// ================= TABLE: POPULATION ======================
// =========================================================

function fillPopulationTable(list) {
    const tb = document.querySelector('#populationTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        if (!b.population || b.population === 0) return;
        const tr = createBaseRow(b, `
            <td>${nz(perTile(b, b.population))}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// ================= TABLE: HAPPINESS =======================
// =========================================================

function fillHappinessTable(list) {
    const tb = document.querySelector('#happinessTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        if (!b.happiness || b.happiness === 0) return;
        const tr = createBaseRow(b, `
            <td>${nz(perTile(b, b.happiness))}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// =================== TABLE: BOOSTS ========================
// =========================================================

function fillBoostsTable(list) {
    const tb = document.querySelector('#boostsTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        const prod = extractAllProductions(b);
        if (prod.fpBoost <= 0 && prod.coinBoost <= 0 && prod.supplyBoost <= 0 && prod.goodsBoost <= 0) return;
        const tr = createBaseRow(b, `
            <td>${nz(prod.fpBoost)}</td>
            <td>${nz(prod.coinBoost)}</td>
            <td>${nz(prod.supplyBoost)}</td>
            <td>${nz(prod.goodsBoost)}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// ================ TABLE: GUILD RAIDS ======================
// =========================================================

function fillGuildRaidsTable(list) {
    const tb = document.querySelector('#guildRaidsTable tbody');
    tb.innerHTML = "";
    const frag = document.createDocumentFragment();

    list.forEach(b => {
        const prod = extractAllProductions(b);
        const hasGR = prod.gr_actions || prod.gr_coins_pct || prod.gr_supplies_pct ||
                      prod.gr_coins_start || prod.gr_supplies_start || prod.gr_goods_start || prod.gr_units_start;
        if (!hasGR) return;
        const tr = createBaseRow(b, `
            <td>${nz(prod.gr_actions)}</td>
            <td>${nz(prod.gr_coins_pct)}</td>
            <td>${nz(prod.gr_supplies_pct)}</td>
            <td>${nz(prod.gr_coins_start)}</td>
            <td>${nz(prod.gr_supplies_start)}</td>
            <td>${nz(prod.gr_goods_start)}</td>
            <td>${nz(prod.gr_units_start)}</td>
        `);
        frag.appendChild(tr);
    });
    tb.appendChild(frag);
}


// =========================================================
// ===================== SORTING ============================
// =========================================================

function enableSorting(id) {
    const table = document.getElementById(id);
    if (!table) return;
    table.querySelectorAll("th").forEach((th, i) => {
        th.onclick = () => sortTable(table, i);
    });
}

function sortTable(table, col) {
    const tb = table.querySelector("tbody");
    const rows = [...tb.querySelectorAll("tr")];

    let asc = table.dataset.sortCol == col && table.dataset.sortDir === "asc" ? false : true;
    table.dataset.sortCol = col;
    table.dataset.sortDir = asc ? "asc" : "desc";

    rows.sort((a, b) => {
        let A = a.children[col]?.innerText.trim() || "";
        let B = b.children[col]?.innerText.trim() || "";

        let nA = parseFloat(A), nB = parseFloat(B);
        if (!isNaN(nA) && !isNaN(nB)) return asc ? nA - nB : nB - nA;

        return asc ? A.localeCompare(B, 'ru') : B.localeCompare(A, 'ru');
    });

    rows.forEach(r => tb.appendChild(r));
}


// =========================================================
// ================== INVENTORY FILTER =====================
// =========================================================

function applyInventoryFilter() {
    document.querySelectorAll("tbody tr").forEach(tr => {
        const invCell = tr.children[1];
        const isInvRow = invCell && invCell.textContent.trim() === "✔";

        if (inventoryMode === 0) {
            tr.dataset.invHidden = "";
        } else if (inventoryMode === 1) {
            tr.dataset.invHidden = isInvRow ? "" : "1";
        } else {
            tr.dataset.invHidden = isInvRow ? "1" : "";
        }
        syncRowVisibility(tr);
    });
}

function syncRowVisibility(tr) {
    const invHidden = tr.dataset.invHidden === "1";
    const favHidden = showFav && !favorites.includes(tr.querySelector(".fav")?.dataset.id);
    const searchVal = document.getElementById("searchInput")?.value.toLowerCase() || "";
    const searchHide = searchVal && !tr.dataset.name?.includes(searchVal);

    tr.style.display = (invHidden || favHidden || searchHide) ? "none" : "";
}


// =========================================================
// ===================== FAVORITE FILTER ====================
// =========================================================

let showFav = false;

document.getElementById("favToggle")?.addEventListener("click", () => {
    showFav = !showFav;
    document.getElementById("favToggle").classList.toggle("active", showFav);
    document.querySelectorAll("tbody tr").forEach(tr => syncRowVisibility(tr));
});
