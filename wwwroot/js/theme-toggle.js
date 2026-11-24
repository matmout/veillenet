// Theme toggle — dark (default) / light.
// The <head> script in _Layout.cshtml applies the saved or preferred theme
// before first paint; this file keeps the toggle button and the theme-color
// meta in sync and handles the switch itself.
(function () {
    'use strict';

    const STORAGE_KEY = 'containsharp-theme';
    // Title-bar colour of each theme (--titlebar-bg), for the browser UI.
    const THEME_COLORS = { dark: '#141618', light: '#e5e8eb' };
    const html = document.documentElement;
    const LIGHT_MQ = window.matchMedia('(prefers-color-scheme: light)');

    // Storage can throw (private mode, blocked site data): the theme still
    // applies to the current page, it is just not remembered.
    function readStored() {
        try {
            const value = localStorage.getItem(STORAGE_KEY);
            return value === 'light' || value === 'dark' ? value : null;
        } catch (e) {
            return null;
        }
    }

    function store(theme) {
        try { localStorage.setItem(STORAGE_KEY, theme); } catch (e) { /* not persisted */ }
    }

    function currentTheme() {
        return html.getAttribute('data-bs-theme') === 'light' ? 'light' : 'dark';
    }

    // The button shows and announces the theme you switch TO.
    function syncControls(theme) {
        const next = theme === 'light' ? 'dark' : 'light';
        const label = 'Switch to ' + next + ' theme';
        const btn = document.getElementById('theme-toggle');
        if (btn) {
            btn.setAttribute('aria-label', label);
            btn.title = label;
        }
        const icon = document.getElementById('theme-icon');
        if (icon) icon.className = next === 'light' ? 'bi bi-sun-fill' : 'bi bi-moon-stars-fill';
        const meta = document.getElementById('meta-theme');
        if (meta) meta.setAttribute('content', THEME_COLORS[theme]);
    }

    function applyTheme(theme) {
        html.setAttribute('data-bs-theme', theme);
        syncControls(theme);
    }

    function switchTheme() {
        const next = currentTheme() === 'dark' ? 'light' : 'dark';
        store(next);

        const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        if (!document.startViewTransition || reduce) {
            applyTheme(next);
            return;
        }
        // Crossfade the whole page as one surface: .theme-switching drops the
        // per-region view-transition names used for page navigation and stops
        // element transitions, so nothing animates underneath the crossfade.
        html.classList.add('theme-switching');
        const transition = document.startViewTransition(function () { applyTheme(next); });
        const cleanup = function () { html.classList.remove('theme-switching'); };
        transition.finished.then(cleanup, cleanup);
    }

    function init() {
        syncControls(currentTheme());
        const btn = document.getElementById('theme-toggle');
        if (btn) btn.addEventListener('click', switchTheme);
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();

    // Follow the OS theme while the visitor has not picked one.
    const onSystemChange = function (e) {
        if (!readStored()) applyTheme(e.matches ? 'light' : 'dark');
    };
    if (LIGHT_MQ.addEventListener) LIGHT_MQ.addEventListener('change', onSystemChange);
    else if (LIGHT_MQ.addListener) LIGHT_MQ.addListener(onSystemChange);
})();
