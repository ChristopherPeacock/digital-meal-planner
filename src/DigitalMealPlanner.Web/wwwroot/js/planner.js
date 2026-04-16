// Digital Meal Planner — planner.js
// Weekly calendar: day-tab navigation, recipe picker sheet, assign/remove

(function () {
    'use strict';

    // ── state ──────────────────────────────────────────────────────────────────
    let _pendingDate    = '';
    let _pendingThemeId = 0;

    // ── helpers ────────────────────────────────────────────────────────────────
    function getToken() {
        return document.querySelector('#af-form [name="__RequestVerificationToken"]')?.value ?? '';
    }

    function post(url, params) {
        params.__RequestVerificationToken = getToken();
        return fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams(params)
        });
    }

    // ── day tabs — scroll the grid to the selected day column ─────────────────
    window.plannerActivateDay = function (dayIdx) {
        // Highlight the tab
        document.querySelectorAll('.day-tab').forEach(btn => {
            const isActive = String(btn.dataset.day) === String(dayIdx);
            btn.classList.toggle('active', isActive);
            btn.setAttribute('aria-selected', String(isActive));
        });

        // Scroll the grid so the selected day column is visible
        const scrollWrap = document.getElementById('planner-scroll-wrap');
        const header = document.querySelector(`.planner-day-hd[data-day="${dayIdx}"]`);
        if (scrollWrap && header) {
            // offsetLeft relative to the grid; subtract the theme label column width
            const labelColWidth = 72 + 3; // 72px + gap
            const targetScroll = header.offsetLeft - labelColWidth;
            scrollWrap.scrollTo({ left: Math.max(0, targetScroll), behavior: 'smooth' });
        }
    };

    // ── sheet open / close ────────────────────────────────────────────────────
    window.plannerOpenSheet = function (btn) {
        _pendingDate    = btn.dataset.date;
        _pendingThemeId = parseInt(btn.dataset.themeId, 10);

        document.getElementById('sheet-title').textContent =
            `${btn.dataset.themeName} \u2014 ${btn.dataset.dayLabel}`;

        const searchEl = document.getElementById('sheet-search');
        searchEl.value = '';
        plannerSearchRecipes('');

        const backdrop = document.getElementById('sheet-backdrop');
        backdrop.hidden = false;
        requestAnimationFrame(() => requestAnimationFrame(() => backdrop.classList.add('open')));
        searchEl.focus();
    };

    window.plannerCloseSheet = function () {
        const backdrop = document.getElementById('sheet-backdrop');
        backdrop.classList.remove('open');
        backdrop.addEventListener('transitionend', function handler() {
            backdrop.hidden = true;
            backdrop.removeEventListener('transitionend', handler);
        });
    };

    // ── recipe search filter ──────────────────────────────────────────────────
    window.plannerSearchRecipes = function (query) {
        const q = query.toLowerCase().trim();
        document.querySelectorAll('.sheet-recipe').forEach(btn => {
            btn.hidden = q.length > 0 && !btn.dataset.recipeTitle.includes(q);
        });
        document.querySelectorAll('.sheet-cookbook').forEach(cb => {
            const anyVisible = [...cb.querySelectorAll('.sheet-recipe')].some(r => !r.hidden);
            cb.style.display = anyVisible ? '' : 'none';
        });
    };

    // ── assign recipe ─────────────────────────────────────────────────────────
    window.plannerPickRecipe = function (recipeId) {
        post('/mealplan/assign', {
            date: _pendingDate,
            themeId: _pendingThemeId,
            recipeId: recipeId
        })
        .then(r => r.ok ? r.json() : r.json().then(e => Promise.reject(e)))
        .then(data => {
            const cell = document.querySelector(
                `.planner-cell[data-date="${_pendingDate}"][data-theme-id="${_pendingThemeId}"]`
            );
            if (cell) {
                const chip = document.createElement('div');
                chip.className = 'cal-entry';
                chip.dataset.entryId = data.entryId;
                chip.style.setProperty('--tc', data.themeColor);
                chip.innerHTML =
                    `<span class="cal-entry-title">${escHtml(data.title)}</span>` +
                    `<button class="cal-entry-remove" aria-label="Remove ${escHtml(data.title)}"` +
                    ` onclick="plannerRemoveEntry(event,${data.entryId})">&#215;</button>`;
                const addBtn = cell.querySelector('.cal-add-btn');
                cell.insertBefore(chip, addBtn);
            }
            plannerCloseSheet();
        })
        .catch(err => alert(err?.error ?? 'Could not assign recipe. Please try again.'));
    };

    // ── remove recipe chip ────────────────────────────────────────────────────
    window.plannerRemoveEntry = function (event, entryId) {
        event.stopPropagation();
        const chip = document.querySelector(`.cal-entry[data-entry-id="${entryId}"]`);
        if (!chip) return;
        post('/mealplan/remove', { entryId: entryId })
            .then(r => { if (r.ok) chip.remove(); })
            .catch(() => alert('Could not remove entry. Please try again.'));
    };

    // ── utilities ─────────────────────────────────────────────────────────────
    function escHtml(str) {
        return str
            .replace(/&/g, '&amp;').replace(/</g, '&lt;')
            .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    // ── init ───────────────────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', function () {
        // Close sheet when tapping the backdrop
        const backdrop = document.getElementById('sheet-backdrop');
        if (backdrop) {
            backdrop.addEventListener('click', e => { if (e.target === backdrop) plannerCloseSheet(); });
        }

        // Scroll to today (or Monday if today is not in this week)
        const grid    = document.getElementById('planner-grid');
        const todayIdx = grid ? parseInt(grid.dataset.todayIdx ?? '-1', 10) : -1;
        plannerActivateDay(todayIdx >= 0 ? todayIdx : 0);
    });
}());


(function () {
    'use strict';

    // ── state ──────────────────────────────────────────────────────────────────
    let _pendingDate    = '';
    let _pendingThemeId = 0;

    // ── helpers ────────────────────────────────────────────────────────────────
    function getToken() {
        return document.querySelector('#af-form [name="__RequestVerificationToken"]')?.value ?? '';
    }

    function post(url, params) {
        params.__RequestVerificationToken = getToken();
        return fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams(params)
        });
    }

    // ── day tabs (mobile) ─────────────────────────────────────────────────────
    window.plannerActivateDay = function (dayIdx) {
        document.querySelectorAll('.planner-day-hd, .planner-cell').forEach(el => {
            el.classList.toggle('day-vis', String(el.dataset.day) === String(dayIdx));
        });
        document.querySelectorAll('.day-tab').forEach(btn => {
            btn.classList.toggle('active', String(btn.dataset.day) === String(dayIdx));
            btn.setAttribute('aria-selected', String(btn.dataset.day) === String(dayIdx));
        });
    };

    // ── sheet open / close ────────────────────────────────────────────────────
    window.plannerOpenSheet = function (btn) {
        _pendingDate    = btn.dataset.date;
        _pendingThemeId = parseInt(btn.dataset.themeId, 10);

        document.getElementById('sheet-title').textContent =
            `Add to ${btn.dataset.dayLabel} \u2014 ${btn.dataset.themeName}`;

        const searchEl = document.getElementById('sheet-search');
        searchEl.value = '';
        plannerSearchRecipes('');

        const backdrop = document.getElementById('sheet-backdrop');
        backdrop.hidden = false;
        // rAF so the hidden→block transition paints before the open class is added
        requestAnimationFrame(() => {
            requestAnimationFrame(() => backdrop.classList.add('open'));
        });
        searchEl.focus();
    };

    window.plannerCloseSheet = function () {
        const backdrop = document.getElementById('sheet-backdrop');
        backdrop.classList.remove('open');
        backdrop.addEventListener('transitionend', function hide() {
            backdrop.hidden = true;
            backdrop.removeEventListener('transitionend', hide);
        });
    };

    // ── recipe search filter ──────────────────────────────────────────────────
    window.plannerSearchRecipes = function (query) {
        const q = query.toLowerCase().trim();
        document.querySelectorAll('.sheet-recipe').forEach(btn => {
            btn.hidden = q.length > 0 && !btn.dataset.recipeTitle.includes(q);
        });
        document.querySelectorAll('.sheet-cookbook').forEach(cb => {
            const anyVisible = [...cb.querySelectorAll('.sheet-recipe')].some(r => !r.hidden);
            cb.style.display = anyVisible ? '' : 'none';
        });
    };

    // ── assign recipe ─────────────────────────────────────────────────────────
    window.plannerPickRecipe = function (recipeId) {
        post('/mealplan/assign', {
            date: _pendingDate,
            themeId: _pendingThemeId,
            recipeId: recipeId
        })
        .then(r => r.ok ? r.json() : r.json().then(e => Promise.reject(e)))
        .then(data => {
            // Find the right cell and inject a chip
            const cell = document.querySelector(
                `.planner-cell[data-date="${_pendingDate}"][data-theme-id="${_pendingThemeId}"]`
            );
            if (cell) {
                const chip = document.createElement('div');
                chip.className  = 'cal-entry';
                chip.dataset.entryId = data.entryId;
                chip.style.setProperty('--tc', data.themeColor);
                chip.innerHTML =
                    `<span class="cal-entry-title">${escHtml(data.title)}</span>` +
                    `<button class="cal-entry-remove" aria-label="Remove ${escHtml(data.title)}"` +
                    ` onclick="plannerRemoveEntry(event,${data.entryId})">&#215;</button>`;
                const addBtn = cell.querySelector('.cal-add-btn');
                cell.insertBefore(chip, addBtn);
            }
            plannerCloseSheet();
        })
        .catch(err => alert(err?.error ?? 'Could not assign recipe. Please try again.'));
    };

    // ── remove recipe chip ────────────────────────────────────────────────────
    window.plannerRemoveEntry = function (event, entryId) {
        event.stopPropagation();
        const chip = document.querySelector(`.cal-entry[data-entry-id="${entryId}"]`);
        if (!chip) return;

        post('/mealplan/remove', { entryId: entryId })
            .then(r => { if (r.ok) chip.remove(); })
            .catch(() => alert('Could not remove entry. Please try again.'));
    };

    // ── init ───────────────────────────────────────────────────────────────────
    function escHtml(str) {
        return str
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    document.addEventListener('DOMContentLoaded', function () {
        // Close sheet when clicking the backdrop
        const backdrop = document.getElementById('sheet-backdrop');
        if (backdrop) {
            backdrop.addEventListener('click', function (e) {
                if (e.target === backdrop) plannerCloseSheet();
            });
        }

        // Activate today's day tab, or Monday if today is not in this week
        const grid    = document.getElementById('planner-grid');
        const todayIdx = grid ? parseInt(grid.dataset.todayIdx ?? '-1', 10) : -1;
        plannerActivateDay(todayIdx >= 0 ? todayIdx : 0);
    });
}());
