/**
 * Version Radar - Interactive Timeline & Filtering
 */
(function () {
    'use strict';

    // Checked when it runs, not once at load
    const prefersReducedMotion = () => window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const canHover = () => window.matchMedia('(hover: hover)').matches;

    const BASE_TOP = 70;
    const ROW_SPACING = 58;
    const BOTTOM_PADDING = 40;

    function setCanvasHeight(canvas, rowCount) {
        const targetHeight = Math.max(260, BASE_TOP + rowCount * ROW_SPACING + BOTTOM_PADDING);
        canvas.style.height = targetHeight + 'px';
        canvas.style.minHeight = targetHeight + 'px';
    }

    // ---- Framework Filter ----
    function filterFramework(framework, btn) {
        // Update the pressed button
        document.querySelectorAll('.filter-pill').forEach(b => {
            const on = b === btn;
            b.classList.toggle('is-active', on);
            b.setAttribute('aria-pressed', on ? 'true' : 'false');
        });

        const showAll = framework === 'all';

        // From now on the draw-in and pulses must not replay on re-shown rows
        document.body.classList.add('radar-filtered');
        hideTooltip();

        // Filter timeline rows
        document.querySelectorAll('.timeline-row').forEach(row => {
            if (showAll || row.dataset.framework === framework) {
                row.classList.remove('hidden-row');
            } else {
                row.classList.add('hidden-row');
            }
        });

        // Recompute visible row slots (CSS slides the rows with a transform)
        let visibleIndex = 0;
        document.querySelectorAll('.timeline-row:not(.hidden-row)').forEach(row => {
            row.style.setProperty('--slot', visibleIndex);
            visibleIndex++;
        });

        // Adjust canvas height (animated only after a filter change)
        const canvas = document.getElementById('timeline-canvas');
        if (canvas) {
            canvas.classList.add('is-animated');
            setCanvasHeight(canvas, visibleIndex);
        }

        // Filter version cards; re-shown cards fade in once
        document.querySelectorAll('.version-card').forEach(card => {
            if (showAll || card.dataset.framework === framework) {
                if (card.classList.contains('hidden-card')) {
                    card.classList.remove('hidden-card');
                    card.classList.remove('card-enter');
                    void card.offsetWidth; // restart the animation
                    card.classList.add('card-enter');
                    const clear = () => {
                        card.removeEventListener('animationend', clear);
                        card.classList.remove('card-enter');
                    };
                    card.addEventListener('animationend', clear);
                    window.setTimeout(clear, 500);
                }
            } else {
                card.classList.add('hidden-card');
                card.classList.remove('card-enter');
            }
        });
    }

    document.querySelectorAll('.filter-pill[data-framework]').forEach(btn => {
        btn.addEventListener('click', () => filterFramework(btn.dataset.framework, btn));
    });

    // ---- Tooltip ----
    const tooltip = document.getElementById('radar-tooltip');
    const tooltipTitle = document.getElementById('tooltip-title');
    const tooltipBadge = document.getElementById('tooltip-badge');
    const tooltipRelease = document.getElementById('tooltip-release');
    const tooltipEos = document.getElementById('tooltip-eos');
    const tooltipSupport = document.getElementById('tooltip-support');
    const tooltipAdoption = document.getElementById('tooltip-adoption');
    const tooltipNote = document.getElementById('tooltip-note');
    const tooltipFeatures = document.getElementById('tooltip-features');
    const tooltipLinks = document.getElementById('tooltip-links');

    function showTooltip(bar, e) {
        if (!tooltip) return;

        tooltipTitle.textContent = bar.dataset.display;

        // Badge (.chip variants only set --chip-token; an empty chip is hidden by CSS)
        tooltipBadge.textContent = bar.dataset.adoption;
        tooltipBadge.className = 'chip tooltip-badge ' + (bar.dataset.adoptionClass || '');

        tooltipRelease.textContent = bar.dataset.release;
        tooltipEos.textContent = bar.dataset.eos;
        tooltipSupport.textContent = bar.dataset.support;

        if (tooltipNote) {
            const supportNote = bar.dataset.supportNote || '';
            tooltipNote.textContent = supportNote;
            tooltipNote.hidden = !supportNote;
        }

        // Adoption row
        tooltipAdoption.textContent = bar.dataset.adoption;
        tooltipAdoption.className = 'chip ' + (bar.dataset.adoptionClass || '');

        // Features
        tooltipFeatures.innerHTML = '';
        const features = (bar.dataset.features || '').split(',');
        features.forEach(f => {
            f = f.trim();
            if (f) {
                const tag = document.createElement('span');
                tag.className = 'chip';
                tag.textContent = f;
                tooltipFeatures.appendChild(tag);
            }
        });

        // Links
        tooltipLinks.innerHTML = '';
        if (bar.dataset.url) {
            const a = document.createElement('a');
            a.href = bar.dataset.url;
            a.className = 'tooltip-link';
            a.target = '_blank';
            a.rel = 'noopener noreferrer';
            a.innerHTML = '<i class="bi bi-box-arrow-up-right"></i> Docs';
            tooltipLinks.appendChild(a);
        }
        if (bar.dataset.migration) {
            const a = document.createElement('a');
            a.href = bar.dataset.migration;
            a.className = 'tooltip-link';
            a.target = '_blank';
            a.rel = 'noopener noreferrer';
            a.innerHTML = '<i class="bi bi-arrow-up-circle"></i> Migration';
            tooltipLinks.appendChild(a);
        }

        // Position first, then fade in (the tooltip is always laid out; only opacity/visibility change)
        positionTooltip(e);
        tooltip.classList.add('is-visible');
    }

    function positionTooltip(e) {
        if (!tooltip) return;
        const pad = 15;
        let x = e.clientX + pad;
        let y = e.clientY + pad;

        const rect = tooltip.getBoundingClientRect();
        const vw = window.innerWidth;
        const vh = window.innerHeight;

        if (x + rect.width > vw - pad) x = e.clientX - rect.width - pad;
        if (y + rect.height > vh - pad) y = e.clientY - rect.height - pad;
        if (x < pad) x = pad;
        if (y < pad) y = pad;

        tooltip.style.transform = 'translate3d(' + Math.round(x) + 'px,' + Math.round(y) + 'px,0)';
    }

    function hideTooltip() {
        if (tooltip) tooltip.classList.remove('is-visible');
    }

    // Keyboard focus has no pointer: anchor the tooltip just under the bar
    function barAnchor(bar) {
        const r = bar.getBoundingClientRect();
        return { clientX: r.left + Math.min(r.width, 120) / 2, clientY: r.bottom - 15 };
    }

    // Attach events to bars
    document.querySelectorAll('.timeline-bar').forEach(bar => {
        bar.addEventListener('mouseenter', function (e) {
            // Touch emulates mouseenter before click: the tooltip would stay stuck, and the card has the details
            if (!canHover()) return;
            showTooltip(this, e);
        });
        bar.addEventListener('mousemove', function (e) {
            if (!canHover()) return;
            positionTooltip(e);
        });
        bar.addEventListener('mouseleave', hideTooltip);

        // Keyboard: focus shows the tooltip, Enter/Space jumps to the card, Escape dismisses
        bar.addEventListener('focus', function () {
            if (this.matches(':focus-visible')) showTooltip(this, barAnchor(this));
        });
        bar.addEventListener('blur', hideTooltip);
        bar.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                hideTooltip();
            } else if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });

        // Click to select version card
        bar.addEventListener('click', function () {
            hideTooltip();

            const selector = `.version-card[data-framework="${this.dataset.framework}"][data-version="${this.dataset.version}"]`;
            const card = document.querySelector(selector);
            if (!card) {
                return;
            }

            document.querySelectorAll('.version-card.selected').forEach(c => c.classList.remove('selected'));
            card.classList.remove('goto-flash');
            void card.offsetWidth; // force reflow so the animation restarts
            card.classList.add('selected', 'goto-flash');
            card.scrollIntoView({ behavior: prefersReducedMotion() ? 'auto' : 'smooth', block: 'center' });

            const clearFlash = () => {
                card.removeEventListener('animationend', clearFlash);
                card.classList.remove('goto-flash');
            };
            card.addEventListener('animationend', clearFlash);
            window.setTimeout(clearFlash, 1000);
        });
    });

    // ---- Drag-to-Scroll (only when content overflows) ----
    const scrollContainer = document.getElementById('timeline-scroll');
    if (scrollContainer) {
        let isDragging = false;
        let startX = 0;
        let scrollLeft = 0;

        function isScrollable() {
            return scrollContainer.scrollWidth > scrollContainer.clientWidth;
        }

        scrollContainer.addEventListener('mousedown', function (e) {
            if (!isScrollable()) return;
            if (e.target.closest('.timeline-bar')) return;
            isDragging = true;
            startX = e.pageX - scrollContainer.offsetLeft;
            scrollLeft = scrollContainer.scrollLeft;
            scrollContainer.style.cursor = 'grabbing';
        });

        scrollContainer.addEventListener('mouseleave', function () {
            if (!isDragging) return;
            isDragging = false;
            scrollContainer.style.cursor = '';
        });

        scrollContainer.addEventListener('mouseup', function () {
            if (!isDragging) return;
            isDragging = false;
            scrollContainer.style.cursor = '';
        });

        scrollContainer.addEventListener('mousemove', function (e) {
            if (!isDragging) return;
            e.preventDefault();
            const x = e.pageX - scrollContainer.offsetLeft;
            const walk = (x - startX) * 1.5;
            scrollContainer.scrollLeft = scrollLeft - walk;
        });

        // On small screens, scroll to center the Today marker
        if (isScrollable()) {
            const todayMarker = document.getElementById('today-marker');
            if (todayMarker) {
                const markerLeft = todayMarker.offsetLeft;
                const containerWidth = scrollContainer.clientWidth;
                scrollContainer.scrollLeft = markerLeft - containerWidth / 2;
            }
        }
    }

    // ---- Initial Layout ----
    // Set canvas height based on number of rows
    const rows = document.querySelectorAll('.timeline-row');
    const canvas = document.getElementById('timeline-canvas');
    if (canvas && rows.length > 0) {
        setCanvasHeight(canvas, rows.length);
    }

})();
