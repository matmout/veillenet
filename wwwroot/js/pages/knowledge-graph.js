/* ===========================================================
   Knowledge graph page
   Loaded with `defer` after three.js, d3, three-spritetext and
   3d-force-graph (deferred scripts run in document order).
   The data endpoint comes from #graph[data-endpoint].
   =========================================================== */
(function () {
    'use strict';

    const graphEl = document.getElementById('graph');
    if (!graphEl) return;

    const panel = document.getElementById('side-panel');
    const content = document.getElementById('panel-content');
    const title = document.getElementById('panel-title');
    const panelClose = document.getElementById('panel-close');
    const infoBox = document.getElementById('info-box');
    const loader = document.getElementById('loader');
    const loaderText = document.getElementById('loader-text');
    const timeline = document.getElementById('timeline');
    const dateLabel = document.getElementById('date-label');
    const nodeCountEl = document.getElementById('node-count');
    const linkCountEl = document.getElementById('link-count');

    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const camMs = () => (reducedMotion.matches ? 0 : 600);

    let graphData = null;
    let Graph = null;
    let selectedId = null;
    let lastFocus = null;

    // ---------- Helpers ----------

    function escapeHtml(value) {
        const div = document.createElement('div');
        div.textContent = value == null ? '' : String(value);
        return div.innerHTML.replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    }

    // Only http(s) URLs may become links; anything else is neutralised.
    function safeUrl(url) {
        try {
            const parsed = new URL(url, window.location.href);
            return parsed.protocol === 'http:' || parsed.protocol === 'https:' ? parsed.href : '#';
        } catch {
            return '#';
        }
    }

    const idOf = (end) => (typeof end === 'object' && end !== null ? end.id : end);

    // Resolves a design token to an rgba() string the WebGL side understands.
    const probeCanvas = document.createElement('canvas');
    probeCanvas.width = probeCanvas.height = 1;
    const probeCtx = probeCanvas.getContext('2d', { willReadFrequently: true });
    function tokenColor(token, alpha = 1) {
        const probe = document.createElement('span');
        probe.style.color = `var(${token})`;
        graphEl.appendChild(probe);
        const computed = getComputedStyle(probe).color;
        probe.remove();
        try {
            probeCtx.clearRect(0, 0, 1, 1);
            probeCtx.fillStyle = computed;
            probeCtx.fillRect(0, 0, 1, 1);
            const [r, g, b] = probeCtx.getImageData(0, 0, 1, 1).data;
            return `rgba(${r}, ${g}, ${b}, ${alpha})`;
        } catch {
            return computed;
        }
    }

    function formatDate(value, options) {
        return new Date(value).toLocaleDateString('en-US', options);
    }

    // ---------- Loader ----------

    function hideLoader() {
        loader.classList.add('is-done');
        setTimeout(() => loader.remove(), 300);
    }

    function showLoadError() {
        loader.classList.add('is-error');
        loaderText.textContent = 'Connection failed';
        if (!loader.querySelector('.loader-retry')) {
            const retry = document.createElement('a');
            retry.className = 'output-action loader-retry';
            retry.dataset.variant = 'primary';
            retry.href = window.location.href;
            retry.textContent = 'Retry';
            loader.querySelector('.loader-container').appendChild(retry);
        }
    }

    // ---------- Init ----------

    async function init() {
        try {
            loaderText.textContent = 'Fetching Knowledge Graph Data';
            const response = await fetch(graphEl.dataset.endpoint);
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

            loaderText.textContent = 'Processing Graph Structure';
            graphData = await response.json();

            nodeCountEl.textContent = graphData.nodes.length;
            linkCountEl.textContent = graphData.links.length;

            renderGraph(graphData);
            initTimeline();
            hideLoader();
        } catch (err) {
            console.error('Failed to load graph data', err);
            showLoadError();
        }
    }

    function renderGraph(data) {
        if (typeof ForceGraph3D === 'undefined' || typeof SpriteText === 'undefined' || typeof d3 === 'undefined') {
            throw new Error('Graph libraries failed to load');
        }

        const byId = new Map(data.nodes.map(n => [n.id, n]));

        const entityLinkCounts = new Map();
        data.links.forEach(link => {
            const sourceId = idOf(link.source);
            const targetId = idOf(link.target);
            const sourceNode = byId.get(sourceId);
            const targetNode = byId.get(targetId);

            if (sourceNode?.type === 'entity' && targetNode?.type === 'article') {
                entityLinkCounts.set(sourceId, (entityLinkCounts.get(sourceId) ?? 0) + 1);
            }
            if (targetNode?.type === 'entity' && sourceNode?.type === 'article') {
                entityLinkCounts.set(targetId, (entityLinkCounts.get(targetId) ?? 0) + 1);
            }
        });

        const spriteColor = tokenColor('--accent');
        const spriteBackground = tokenColor('--panel', 0.8);
        const linkColor = tokenColor('--border-strong', 0.6);
        const particleColor = tokenColor('--token-keyword');

        const touchesSelected = (link) =>
            selectedId !== null && (idOf(link.source) === selectedId || idOf(link.target) === selectedId);

        Graph = ForceGraph3D()(graphEl)
            .width(graphEl.clientWidth)
            .height(graphEl.clientHeight)
            .graphData(data)
            .backgroundColor('rgba(0,0,0,0)')
            .nodeLabel('label')
            .nodeAutoColorBy('type')
            .nodeVal(node => {
                if (node.type === 'entity') {
                    const count = entityLinkCounts.get(node.id) ?? 0;
                    return 4 + Math.min(count, 20) * 0.8;
                }
                return 2;
            })
            .linkWidth(link => {
                // Thicker links for stronger connections
                const sourceNode = byId.get(idOf(link.source));
                const targetNode = byId.get(idOf(link.target));
                return sourceNode?.type === 'entity' && targetNode?.type === 'entity' ? 2 : 1;
            })
            // Particles only run on the links of the selected node, never as ambient motion.
            .linkDirectionalParticles(link => (!reducedMotion.matches && touchesSelected(link) ? 3 : 0))
            .linkDirectionalParticleSpeed(0.008)
            .linkDirectionalParticleWidth(2)
            .linkDirectionalParticleColor(() => particleColor)
            .linkColor(() => linkColor)
            .cooldownTime(reducedMotion.matches ? 0 : 4000)
            .warmupTicks(reducedMotion.matches ? 120 : 0)
            .nodeThreeObject(node => {
                if (node.type === 'entity') {
                    const count = entityLinkCounts.get(node.id) ?? 0;
                    const sizeBoost = Math.min(count, 20) * 0.3;
                    const sprite = new SpriteText(node.label);
                    sprite.color = spriteColor;
                    sprite.textHeight = 6 + sizeBoost;
                    sprite.backgroundColor = spriteBackground;
                    sprite.padding = 4;
                    sprite.borderRadius = 8;
                    sprite.fontFace = 'Inter';
                    return sprite;
                }
                return false;
            })
            .onNodeClick(node => {
                const distance = 120;
                const distRatio = 1 + distance / Math.hypot(node.x, node.y, node.z);

                Graph.cameraPosition(
                    { x: node.x * distRatio, y: node.y * distRatio, z: node.z * distRatio },
                    node,
                    camMs()
                );

                setSelected(node.id);
                if (node.type === 'article') {
                    openArticlePanel(node);
                } else if (node.type === 'entity') {
                    openEntityPanel(node);
                }
            })
            .onNodeHover(node => {
                graphEl.style.cursor = node ? 'pointer' : '';
            });

        // Enhanced physics
        Graph.d3Force('charge').strength(-150);
        Graph.d3Force('link').distance(60);
        Graph.d3Force('center', d3.forceCenter(0, 0, 0));

        // Collision detection for better spacing
        Graph.d3Force('collide', d3.forceCollide().radius(node => (node.type === 'entity' ? 20 : 8)).strength(0.5));

        // Keep the canvas matched to its container.
        if (typeof ResizeObserver !== 'undefined') {
            new ResizeObserver(() => {
                if (Graph && graphEl.clientWidth && graphEl.clientHeight) {
                    Graph.width(graphEl.clientWidth).height(graphEl.clientHeight);
                }
            }).observe(graphEl);
        }

        // Do not render while the tab is hidden.
        document.addEventListener('visibilitychange', () => {
            if (!Graph) return;
            if (document.hidden) Graph.pauseAnimation();
            else Graph.resumeAnimation();
        });
    }

    function setSelected(id) {
        selectedId = id;
        if (Graph) Graph.linkDirectionalParticles(Graph.linkDirectionalParticles());
    }

    function resetCamera() {
        if (!Graph) return;
        Graph.cameraPosition({ x: 0, y: 0, z: 300 }, { x: 0, y: 0, z: 0 }, camMs());
    }

    // ---------- Side panel ----------

    function openPanel() {
        if (!panel.classList.contains('open')) {
            lastFocus = document.activeElement;
        }
        panel.classList.add('open');
        panel.scrollTop = 0;
        panelClose.focus({ preventScroll: true });
    }

    function closePanel() {
        if (!panel.classList.contains('open')) return;
        panel.classList.remove('open');
        setSelected(null);
        if (lastFocus && lastFocus !== document.body && document.contains(lastFocus)) {
            lastFocus.focus({ preventScroll: true });
        }
        lastFocus = null;
    }

    function openArticlePanel(node) {
        title.textContent = node.label;
        // node.summary is server-side sanitised (AI HTML) or HTML-encoded (plain fallback): inserted as is.
        content.innerHTML = `
            <div class="article-card">
                <div class="article-meta">
                    <span class="chip chip--pill meta-tag">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                            <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                            <circle cx="12" cy="7" r="4"></circle>
                        </svg>
                        ${escapeHtml(node.source)}
                    </span>
                    <span class="chip chip--pill meta-tag date">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                            <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
                            <line x1="16" y1="2" x2="16" y2="6"></line>
                            <line x1="8" y1="2" x2="8" y2="6"></line>
                            <line x1="3" y1="10" x2="21" y2="10"></line>
                        </svg>
                        ${escapeHtml(formatDate(node.date, { month: 'long', day: 'numeric', year: 'numeric' }))}
                    </span>
                </div>
                <div class="article-summary">
                    ${node.summary ?? ''}
                </div>
                <a href="${escapeHtml(safeUrl(node.url))}" target="_blank" rel="noopener noreferrer" class="output-action btn-visit" data-variant="primary">
                    Read Original Article
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                        <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"></path>
                        <polyline points="15 3 21 3 21 9"></polyline>
                        <line x1="10" y1="14" x2="21" y2="3"></line>
                    </svg>
                </a>
            </div>
        `;
        openPanel();
    }

    function openEntityPanel(node) {
        title.textContent = node.label;

        let articlesHtml;
        if (node.articles && node.articles.length > 0) {
            const sortedArticles = [...node.articles].sort((a, b) => new Date(b.date) - new Date(a.date));

            articlesHtml = sortedArticles.map(article => `
                <a class="entity-article-item" href="${escapeHtml(safeUrl(article.url))}" target="_blank" rel="noopener noreferrer">
                    <h4>${escapeHtml(article.title)}</h4>
                    <div class="item-meta">
                        <span>${escapeHtml(article.source)}</span>
                        <span aria-hidden="true">•</span>
                        <span>${escapeHtml(formatDate(article.date, { month: 'short', day: 'numeric' }))}</span>
                    </div>
                    <div class="item-summary">
                        ${article.summary ?? ''}
                    </div>
                    <span class="read-more">
                        Read article
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                            <line x1="5" y1="12" x2="19" y2="12"></line>
                            <polyline points="12 5 19 12 12 19"></polyline>
                        </svg>
                    </span>
                </a>
            `).join('');
        } else {
            articlesHtml = '<p class="empty-state" role="status">No articles found for this topic.</p>';
        }

        content.innerHTML = `
            <div class="entity-stats">
                <div class="entity-stats-icon">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                        <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                        <polyline points="14 2 14 8 20 8"></polyline>
                        <line x1="16" y1="13" x2="8" y2="13"></line>
                        <line x1="16" y1="17" x2="8" y2="17"></line>
                        <polyline points="10 9 9 9 8 9"></polyline>
                    </svg>
                </div>
                <div class="entity-stats-text">
                    <div class="entity-stats-count">${node.articles ? node.articles.length : 0}</div>
                    <div class="entity-stats-label">Related Articles</div>
                </div>
            </div>
            <div class="articles-list">
                ${articlesHtml}
            </div>
        `;
        openPanel();
    }

    // ---------- Timeline ----------

    function initTimeline() {
        if (!graphData || !graphData.nodes) return;

        const articleNodes = graphData.nodes.filter(n => n.type === 'article');
        if (articleNodes.length === 0) return;

        const byId = new Map(graphData.nodes.map(n => [n.id, n]));
        const dates = articleNodes.map(n => new Date(n.date).getTime()).sort((a, b) => a - b);
        const minDate = dates[0];
        const maxDate = dates[dates.length - 1];

        function applyFilter() {
            const percent = timeline.value / 100;
            const threshold = minDate + (maxDate - minDate) * percent;

            dateLabel.textContent = formatDate(threshold, { month: 'short', day: 'numeric' });

            const links = graphData.links.filter(l => {
                const sourceNode = byId.get(idOf(l.source));
                const targetNode = byId.get(idOf(l.target));
                if (!sourceNode || !targetNode) return false;

                const isSourceOk = sourceNode.type !== 'article' || new Date(sourceNode.date).getTime() <= threshold;
                const isTargetOk = targetNode.type !== 'article' || new Date(targetNode.date).getTime() <= threshold;
                return isSourceOk && isTargetOk;
            });

            const activeNodeIds = new Set();
            links.forEach(l => {
                activeNodeIds.add(idOf(l.source));
                activeNodeIds.add(idOf(l.target));
            });

            const nodes = graphData.nodes.filter(n => {
                if (n.type === 'entity') return activeNodeIds.has(n.id);
                return new Date(n.date).getTime() <= threshold;
            });

            Graph.graphData({ nodes, links });

            nodeCountEl.textContent = nodes.length;
            linkCountEl.textContent = links.length;
        }

        // One graph rebuild per frame, however many input events fire.
        let raf = 0;
        timeline.addEventListener('input', () => {
            cancelAnimationFrame(raf);
            raf = requestAnimationFrame(applyFilter);
        });
    }

    // ---------- Wiring ----------

    document.getElementById('info-close').addEventListener('click', () => {
        infoBox.hidden = true;
    });
    document.getElementById('reset-view').addEventListener('click', resetCamera);
    panelClose.addEventListener('click', closePanel);

    document.addEventListener('keydown', (e) => {
        if (e.ctrlKey || e.metaKey || e.altKey) return;
        if (e.key === 'Escape') closePanel();
        if (e.key === 'r' || e.key === 'R') resetCamera();
    });

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
