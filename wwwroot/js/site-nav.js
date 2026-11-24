// Site navigation: desktop sub-tab popovers and the phone nav sheet.
//
// Desktop (min-width: 769px): each .ide-tab-group shows its .ide-subtabs
// popover on hover (fine pointers only, see site.css), on keyboard focus
// (:focus-within) and on .open. A first TAP on a tab opens the popover
// instead of navigating (touch screens have no hover); mouse clicks and the
// keyboard navigate straight away. Escape closes the popover and returns
// focus to its tab (.dismissed keeps it closed while focus stays inside).
//
// Phone (max-width: 768px): the tab bar is a sheet under the title bar,
// opened by #ide-mobile-toggle. While it is open the page behind is inert and
// does not scroll; Escape or a click on the scrim (.ide-chrome::after) closes
// it and gives focus back to the toggle.
(function () {
    'use strict';

    const bar = document.getElementById('ide-tabbar');
    const chrome = document.querySelector('.ide-chrome');
    if (!bar || !chrome) return;

    const html = document.documentElement;
    const toggle = document.getElementById('ide-mobile-toggle');
    const toggleIcon = toggle ? toggle.querySelector('.bi') : null;
    const groups = Array.from(bar.querySelectorAll('.ide-tab-group'));
    const NAV_MQ = window.matchMedia('(max-width: 768px)');

    function onMediaChange(mq, handler) {
        if (mq.addEventListener) mq.addEventListener('change', handler);
        else if (mq.addListener) mq.addListener(handler); // Safari < 14
    }

    /* ---------------- Phone sheet ---------------- */

    function isSheetOpen() {
        return bar.classList.contains('open');
    }

    // Page regions that must not be reachable while the sheet covers them.
    function backgroundRegions() {
        return [document.getElementById('main-content'), document.querySelector('.ide-statusbar')]
            .filter(Boolean);
    }

    // opts.instant: skip the fade (used right before a navigation so the
    //               cross-document view-transition snapshot never catches it).
    // opts.returnFocus: give focus back to the toggle (Escape, scrim click).
    function setSheet(open, opts) {
        opts = opts || {};
        if (!toggle || open === isSheetOpen()) return;

        if (opts.instant) bar.classList.add('ide-tabbar-instant');

        bar.classList.toggle('open', open);
        html.classList.toggle('ide-nav-open', open);
        toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
        toggle.setAttribute('aria-label', open ? 'Close navigation' : 'Open navigation');
        if (toggleIcon) toggleIcon.className = open ? 'bi bi-x-lg' : 'bi bi-list';
        backgroundRegions().forEach(function (el) { el.inert = open; });

        if (open) {
            const target = bar.querySelector('.ide-subtab[aria-current="page"]') || bar.querySelector('a[href]');
            if (target) target.focus({ preventScroll: true });
        } else if (opts.returnFocus) {
            toggle.focus({ preventScroll: true });
        }

        if (opts.instant) {
            // Let the instant state paint once, then restore the transitions.
            requestAnimationFrame(function () {
                requestAnimationFrame(function () { bar.classList.remove('ide-tabbar-instant'); });
            });
        }
    }

    if (toggle) {
        toggle.addEventListener('click', function () {
            setSheet(!isSheetOpen());
        });
    }

    // The scrim is .ide-chrome::after: its clicks target the chrome element itself.
    chrome.addEventListener('click', function (e) {
        if (e.target === chrome && isSheetOpen()) setSheet(false, { returnFocus: true });
    });

    // Following a link from the sheet: close it at once, before the page swaps.
    bar.addEventListener('click', function (e) {
        if (e.defaultPrevented || !NAV_MQ.matches || !isSheetOpen()) return;
        const link = e.target.closest('a[href]');
        if (link) setSheet(false, { instant: true });
    });

    function closeSheetInstantly() {
        if (isSheetOpen()) setSheet(false, { instant: true });
    }
    window.addEventListener('pageswap', closeSheetInstantly);
    window.addEventListener('pagehide', closeSheetInstantly);

    /* ---------------- Desktop popovers ---------------- */

    function tabOf(group) {
        return group.querySelector('.ide-tab');
    }

    // aria-expanded only describes the desktop popover; on phones every
    // sub-tab is always listed in the sheet, so the attribute is removed.
    function syncExpanded(group) {
        const tab = tabOf(group);
        if (!tab) return;
        if (NAV_MQ.matches) {
            tab.removeAttribute('aria-expanded');
            return;
        }
        const shown = !group.classList.contains('dismissed') &&
            (group.classList.contains('open') || group.contains(document.activeElement));
        tab.setAttribute('aria-expanded', shown ? 'true' : 'false');
    }

    function closeGroups(except) {
        groups.forEach(function (g) {
            if (g === except) return;
            g.classList.remove('open');
            syncExpanded(g);
        });
    }

    // Remember how the last interaction started: only touch and pen taps
    // need the "first tap opens the popover" behaviour.
    let lastPointerType = 'mouse';
    bar.addEventListener('pointerdown', function (e) {
        lastPointerType = e.pointerType || 'mouse';
    }, true);

    groups.forEach(function (group) {
        const tab = tabOf(group);
        if (!tab) return;

        tab.addEventListener('click', function (e) {
            if (NAV_MQ.matches) return;                    // sheet: plain links
            const keyboard = e.detail === 0;               // Enter on a focused tab
            if (keyboard || lastPointerType === 'mouse') { // popover already shown by hover/focus
                closeGroups(group);
                return;
            }
            if (!group.classList.contains('open')) {
                e.preventDefault();
                closeGroups(group);
                group.classList.remove('dismissed');
                group.classList.add('open');
                syncExpanded(group);
            }
        });

        group.addEventListener('focusin', function () {
            closeGroups(group);                            // only one popover at a time
            syncExpanded(group);
        });

        group.addEventListener('focusout', function (e) {
            if (group.contains(e.relatedTarget)) return;
            group.classList.remove('dismissed', 'open');
            syncExpanded(group);
        });

        group.addEventListener('mouseenter', function () {
            if (!NAV_MQ.matches) closeGroups(group);
        });

        group.addEventListener('mouseleave', function () {
            group.classList.remove('dismissed');
        });

        syncExpanded(group);
    });

    // A click anywhere outside the tab bar closes tapped-open popovers.
    document.addEventListener('click', function (e) {
        if (!bar.contains(e.target)) closeGroups(null);
    });

    /* ---------------- Keyboard and viewport changes ---------------- */

    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape') return;

        if (isSheetOpen()) {
            setSheet(false, { returnFocus: true });
            return;
        }
        if (NAV_MQ.matches) return;

        const active = document.activeElement;
        const group = groups.find(function (g) { return g.contains(active); }) ||
            groups.find(function (g) { return g.classList.contains('open'); });
        if (!group) return;

        group.classList.remove('open');
        group.classList.add('dismissed');
        syncExpanded(group);
        const tab = tabOf(group);
        if (tab && group.contains(active) && active !== tab) tab.focus({ preventScroll: true });
    });

    onMediaChange(NAV_MQ, function () {
        closeSheetInstantly();
        groups.forEach(function (g) { g.classList.remove('open', 'dismissed'); });
        groups.forEach(syncExpanded);
    });
})();
