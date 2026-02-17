/**
 * Version Radar - Interactive Timeline & Filtering
 */
(function () {
    'use strict';

    // ---- Framework Filter ----
    window.filterFramework = function (framework, btn) {
        // Update active button
        document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');

        const showAll = framework === 'all';

        // Filter timeline rows
        document.querySelectorAll('.timeline-row').forEach(row => {
            if (showAll || row.dataset.framework === framework) {
                row.classList.remove('hidden-row');
            } else {
                row.classList.add('hidden-row');
            }
        });

        // Recompute visible row positions
        const baseTop = 70;
        const rowSpacing = 58;
        const bottomPadding = 40;
        let visibleIndex = 0;
        document.querySelectorAll('.timeline-row:not(.hidden-row)').forEach(row => {
            row.style.top = (baseTop + visibleIndex * rowSpacing) + 'px';
            visibleIndex++;
        });

        // Adjust canvas height
        const canvas = document.getElementById('timeline-canvas');
        if (canvas) {
            const targetHeight = Math.max(260, baseTop + visibleIndex * rowSpacing + bottomPadding);
            canvas.style.height = targetHeight + 'px';
            canvas.style.minHeight = targetHeight + 'px';
        }

        // Filter version cards
        document.querySelectorAll('.version-card').forEach(card => {
            if (showAll || card.dataset.framework === framework) {
                card.classList.remove('hidden-card');
            } else {
                card.classList.add('hidden-card');
            }
        });
    };

    // ---- Tooltip ----
    const tooltip = document.getElementById('radar-tooltip');
    const tooltipTitle = document.getElementById('tooltip-title');
    const tooltipBadge = document.getElementById('tooltip-badge');
    const tooltipRelease = document.getElementById('tooltip-release');
    const tooltipEos = document.getElementById('tooltip-eos');
    const tooltipSupport = document.getElementById('tooltip-support');
    const tooltipAdoption = document.getElementById('tooltip-adoption');
    const tooltipFeatures = document.getElementById('tooltip-features');
    const tooltipLinks = document.getElementById('tooltip-links');

    function showTooltip(bar, e) {
        if (!tooltip) return;

        tooltipTitle.textContent = bar.dataset.display;

        // Badge
        tooltipBadge.textContent = bar.dataset.adoption;
        tooltipBadge.className = 'tooltip-badge badge-adoption ' + (bar.dataset.adoptionClass || '');

        tooltipRelease.textContent = bar.dataset.release;
        tooltipEos.textContent = bar.dataset.eos;
        tooltipSupport.textContent = bar.dataset.support;

        // Adoption row
        tooltipAdoption.textContent = bar.dataset.adoption;
        tooltipAdoption.className = 'badge-adoption ' + (bar.dataset.adoptionClass || '');

        // Features
        tooltipFeatures.innerHTML = '';
        const features = (bar.dataset.features || '').split(',');
        features.forEach(f => {
            f = f.trim();
            if (f) {
                const tag = document.createElement('span');
                tag.className = 'tooltip-feature-tag';
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

        tooltip.style.display = 'block';
        positionTooltip(e);
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

        tooltip.style.left = x + 'px';
        tooltip.style.top = y + 'px';
    }

    function hideTooltip() {
        if (tooltip) tooltip.style.display = 'none';
    }

    // Attach events to bars
    document.querySelectorAll('.timeline-bar').forEach(bar => {
        bar.addEventListener('mouseenter', function (e) {
            showTooltip(this, e);
        });
        bar.addEventListener('mousemove', function (e) {
            positionTooltip(e);
        });
        bar.addEventListener('mouseleave', hideTooltip);

        // Click to select version card
        bar.addEventListener('click', function () {
            const selector = `.version-card[data-framework="${this.dataset.framework}"][data-version="${this.dataset.version}"]`;
            const card = document.querySelector(selector);
            if (!card) {
                return;
            }

            document.querySelectorAll('.version-card.selected').forEach(c => c.classList.remove('selected'));
            card.classList.add('selected', 'shake');
            card.scrollIntoView({ behavior: 'smooth', block: 'center' });

            window.setTimeout(() => {
                card.classList.remove('shake');
            }, 400);
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
        const baseTop = 70;
        const rowSpacing = 58;
        const bottomPadding = 40;
        const targetHeight = Math.max(260, baseTop + rows.length * rowSpacing + bottomPadding);
        canvas.style.height = targetHeight + 'px';
        canvas.style.minHeight = targetHeight + 'px';
    }

})();
