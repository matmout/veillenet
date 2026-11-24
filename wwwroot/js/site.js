// Dashboard hero: "today_in_60_seconds.log".
// Prints the daily briefing like build output, once per briefing, then stays static.
// Skippable (click / Escape), replayable (reload button), instant under reduced motion.
(function () {
  const el = document.getElementById('output-stream');
  if (!el) return;

  // Read at the moment of use: the preference can change while the page is open.
  const reduce = () => window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  const SEEN_KEY = 'cs-briefing-seen';
  const briefing = (typeof window.__dailyBriefing === 'string' && window.__dailyBriefing.trim().length > 0)
    ? window.__dailyBriefing
    : null;
  const briefingDate = typeof window.__dailyBriefingDate === 'string' ? window.__dailyBriefingDate : '';

  const typer = document.querySelector('.dev-typer');
  const cursorChip = document.getElementById('status-cursor');

  const timestamp = () => new Date().toISOString().replace('T', ' ').replace('Z', '');

  function buildLines() {
    if (!briefing) {
      return [{ text: 'No briefing available yet.', cls: 'dim', wait: 0 }];
    }
    const bullets = briefing.split('\n').map(l => l.trim()).filter(l => l.length > 0);
    const bulletCount = Math.max(1, bullets.filter(l => l.startsWith('•')).length);
    // Keep the whole sequence under ~2s whatever the number of bullets.
    const bulletWait = Math.min(220, Math.round(1600 / bulletCount));
    // stamp: the [timestamp] prefix is added when the line is printed.
    const lines = [
      { text: 'Generating AI daily briefing...', stamp: true, cls: 'dim', wait: 80 },
      { text: '', cls: '', wait: 80 }
    ];
    bullets.forEach(line => {
      lines.push({ text: line, cls: line.startsWith('•') ? 'ok' : 'dim', wait: bulletWait });
    });
    lines.push({ text: '', cls: '', wait: 80 });
    lines.push({ text: 'Briefing ready, updated once daily.', stamp: true, cls: 'dim', wait: 0 });
    return lines;
  }

  function appendLine(line) {
    const div = document.createElement('div');
    if (line.cls) div.className = line.cls;
    div.textContent = line.stamp ? `[${timestamp()}] ${line.text}` : line.text;
    el.appendChild(div);
    return div.textContent;
  }

  function setCursor(lineCount, lastText) {
    if (!cursorChip) return;
    cursorChip.textContent = `Ln ${lineCount}, Col ${lastText.length + 1}`;
  }

  function addCaret() {
    const rows = Array.from(el.children).filter(d => d.textContent.length > 0);
    const last = rows[rows.length - 1];
    if (!last) return;
    const caret = document.createElement('span');
    caret.className = 'output-caret';
    caret.setAttribute('aria-hidden', 'true');
    caret.textContent = '▍';
    last.appendChild(caret);
  }

  let run = null; // { lines, index, timer }

  function finish() {
    if (run && run.timer) clearTimeout(run.timer);
    run = null;
    el.setAttribute('aria-busy', 'false');
    if (typer) typer.classList.remove('is-typing');
    if (cursorChip) cursorChip.textContent = 'Ln 1, Col 1';
    addCaret();
    el.scrollTop = el.scrollHeight;
  }

  function renderStatic() {
    el.innerHTML = '';
    buildLines().forEach(appendLine);
    finish();
  }

  function skip() {
    if (!run) return;
    for (let i = run.index; i < run.lines.length; i++) appendLine(run.lines[i]);
    finish();
  }

  function step() {
    if (!run) return;
    const line = run.lines[run.index];
    const printed = appendLine(line);
    run.index++;
    setCursor(el.children.length, printed);
    el.scrollTop = el.scrollHeight;
    if (run.index >= run.lines.length) {
      finish();
      return;
    }
    run.timer = setTimeout(step, line.wait);
  }

  function play() {
    if (run && run.timer) clearTimeout(run.timer);
    el.innerHTML = '';
    run = { lines: buildLines(), index: 0, timer: null };
    el.setAttribute('aria-busy', 'true');
    if (typer) typer.classList.add('is-typing');
    step();
  }

  function markSeen() {
    try { localStorage.setItem(SEEN_KEY, briefingDate); } catch { /* storage unavailable */ }
  }

  function alreadySeen() {
    try { return briefingDate !== '' && localStorage.getItem(SEEN_KEY) === briefingDate; } catch { return false; }
  }

  // Skip on click in the log or Escape while printing.
  el.addEventListener('click', skip);
  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape' && run) skip();
  });

  // Reload button replays the sequence.
  const reloadBtn = document.querySelector('.output-action[data-action="reload"]');
  if (reloadBtn) {
    reloadBtn.addEventListener('click', function () {
      if (reduce() || !briefing) renderStatic(); else play();
    });
  }

  if (!briefing || reduce() || alreadySeen()) {
    renderStatic();
  } else {
    play();
    markSeen();
  }
})();

// Form feedback: bring a server-rendered alert into view, and show a pending
// state on the submit button of POST forms (restored after a bfcache return).
(function () {
  const reduce = () => window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  document.querySelectorAll('.vs-alert[role="alert"]').forEach(function (alert) {
    alert.scrollIntoView({ block: 'center', behavior: reduce() ? 'auto' : 'smooth' });
  });

  document.querySelectorAll('form[method="post" i]').forEach(function (form) {
    form.addEventListener('submit', function (e) {
      const btn = form.querySelector('[type="submit"]');
      if (!btn) return;
      // Next tick: every submit handler has run by then, so a submission
      // cancelled by client-side validation (jquery-validate) is left alone,
      // and the button's own name/value has already been submitted.
      setTimeout(function () {
        if (e.defaultPrevented) return;
        btn.classList.add('is-pending');
        btn.setAttribute('aria-busy', 'true');
        btn.disabled = true;
      }, 0);
    });
  });

  window.addEventListener('pageshow', function (e) {
    if (!e.persisted) return;
    document.querySelectorAll('.is-pending').forEach(function (btn) {
      btn.classList.remove('is-pending');
      btn.removeAttribute('aria-busy');
      btn.disabled = false;
    });
  });
})();
