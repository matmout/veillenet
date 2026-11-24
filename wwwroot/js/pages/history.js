(() => {
    const resultsEl = document.getElementById('results');
    const totalEl = document.getElementById('totalCount');
    const formEl = document.getElementById('history-filter');
    const keywordEl = document.getElementById('keyword');
    const startDateEl = document.getElementById('startDate');
    const endDateEl = document.getElementById('endDate');
    const publisherEl = document.getElementById('publisher');
    const searchBtn = document.getElementById('searchBtn');
    const resetBtn = document.getElementById('resetBtn');
    const prevBtn = document.getElementById('prevPage');
    const nextBtn = document.getElementById('nextPage');
    if (!resultsEl || !totalEl || !formEl) return;

    const fallbackImg = resultsEl.dataset.fallbackImg || '';
    const aiIcon = resultsEl.dataset.aiIcon || '';

    let page = 1;
    const pageSize = 20;
    let total = parseInt(totalEl.textContent || '0');
    let requestId = 0;

    const escapeHtml = (s) => (s || '').replace(/[&<>"']/g, c => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[c]));

    // Feed URLs are untrusted: only http(s) links are rendered as links.
    const safeUrl = (u) => {
        try {
            const protocol = new URL(u, location.href).protocol;
            return protocol === 'http:' || protocol === 'https:' ? u : '#';
        } catch {
            return '#';
        }
    };

    const renderItems = (items) => {
        if (!items || !items.length) {
            resultsEl.innerHTML = '<div class="empty-state" role="status">No articles found.</div>';
            return;
        }

        resultsEl.innerHTML = items.map(n => `
<article class="content-card history-card" data-token="keyword">
  <div class="history-card-body">
    <img class="history-thumb" src="${escapeHtml(n.image || fallbackImg)}" alt="${escapeHtml(n.title)}" loading="lazy" />
    <div class="history-card-content">
      <div class="history-card-head">
        <h3 class="history-card-title"><a href="${escapeHtml(safeUrl(n.url))}" target="_blank" rel="noopener noreferrer">${escapeHtml(n.title)}</a></h3>
        <div class="history-card-badges">
          ${n.hasSummary ? `<a href="/AiSummary?url=${encodeURIComponent(n.url)}" class="history-ai-link" aria-label="AI summary"><img src="${aiIcon}" alt="" /></a>` : ''}
          <span class="timeline-chip" data-token="attribute">${escapeHtml(n.source)}</span>
        </div>
      </div>
      <p class="history-card-summary">${n.summary || ''}</p>
      <div class="timeline-meta"><span><i class="bi bi-calendar"></i>${escapeHtml(n.publishedDate)}</span></div>
    </div>
  </div>
</article>`).join('');
    };

    const updatePagingButtons = () => {
        prevBtn.disabled = page <= 1;
        const maxPage = Math.ceil(total / pageSize) || 1;
        nextBtn.disabled = page >= maxPage;
    };

    const ensureStartDate = () => {
        if (!startDateEl.value) {
            const d = new Date();
            d.setDate(d.getDate() - 30);
            startDateEl.value = d.toISOString().split('T')[0];
        }
        return startDateEl.value;
    };

    const buildQuery = () => {
        const params = new URLSearchParams();
        if (keywordEl.value.trim()) params.append('keyword', keywordEl.value.trim());
        const startVal = ensureStartDate();
        if (startVal) params.append('startDate', startVal);
        if (endDateEl.value) params.append('endDate', endDateEl.value);
        if (publisherEl.value) params.append('source', publisherEl.value);
        params.append('page', page);
        params.append('pageSize', pageSize);
        return params.toString();
    };

    const scrollToResults = () => {
        const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        resultsEl.scrollIntoView({ behavior: reduced ? 'auto' : 'smooth', block: 'start' });
    };

    const load = async ({ scroll = false } = {}) => {
        const myRequest = ++requestId;
        const url = `?handler=Search&${buildQuery()}`;
        resultsEl.setAttribute('aria-busy', 'true');
        resultsEl.classList.add('is-loading');
        try {
            const res = await fetch(url);
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();
            if (myRequest !== requestId) return; // a newer search superseded this one
            total = data.total || 0;
            totalEl.textContent = total;
            resultsEl.classList.add('is-swapped');
            renderItems(data.items || []);
            updatePagingButtons();
            if (scroll) scrollToResults();
        } catch (err) {
            if (myRequest !== requestId) return;
            resultsEl.innerHTML = '<div class="empty-state" data-tone="error" role="alert">Error loading results.</div>';
        } finally {
            if (myRequest === requestId) {
                resultsEl.classList.remove('is-loading');
                resultsEl.removeAttribute('aria-busy');
            }
        }
    };

    formEl.addEventListener('submit', (e) => e.preventDefault());
    searchBtn.addEventListener('click', () => { page = 1; load(); });
    resetBtn.addEventListener('click', (e) => {
        // The button is type="reset": stop the native reset, which would run
        // after load() and blank the start date that the query actually used.
        e.preventDefault();
        keywordEl.value = '';
        startDateEl.value = '';
        endDateEl.value = '';
        publisherEl.value = '';
        page = 1;
        load();
    });
    prevBtn.addEventListener('click', () => { if (page > 1) { page--; load({ scroll: true }); } });
    nextBtn.addEventListener('click', () => { page++; load({ scroll: true }); });

    keywordEl.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') { e.preventDefault(); page = 1; load(); }
    });

    ensureStartDate();
})();
