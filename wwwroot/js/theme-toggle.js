// Theme toggle — dark (default) / light
(function () {
    'use strict';

    const STORAGE_KEY = 'containsharp-theme';
    const html = document.documentElement;

    function getPreferredTheme() {
        const stored = localStorage.getItem(STORAGE_KEY);
        if (stored) return stored;
        return window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
    }

    function applyTheme(theme) {
        html.setAttribute('data-bs-theme', theme);
        // Update toggle icon
        const icon = document.getElementById('theme-icon');
        if (icon) {
            icon.className = theme === 'light' ? 'bi bi-sun-fill' : 'bi bi-moon-stars-fill';
        }
    }

    // Apply immediately (before DOMContentLoaded) to prevent FOUC
    applyTheme(getPreferredTheme());

    // Toggle handler — wired after DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        const btn = document.getElementById('theme-toggle');
        if (!btn) return;

        // Ensure icon matches
        applyTheme(getPreferredTheme());

        btn.addEventListener('click', function () {
            const current = html.getAttribute('data-bs-theme') || 'dark';
            const next = current === 'dark' ? 'light' : 'dark';
            localStorage.setItem(STORAGE_KEY, next);
            applyTheme(next);
        });
    });

    // React to OS theme change if user hasn't set a manual preference
    window.matchMedia('(prefers-color-scheme: light)').addEventListener('change', function (e) {
        if (!localStorage.getItem(STORAGE_KEY)) {
            applyTheme(e.matches ? 'light' : 'dark');
        }
    });
})();
