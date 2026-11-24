(() => {
    // After SubmitAnswer the page re-renders from the top: bring the verdict into view.
    const result = document.getElementById('result');
    if (!result) return;

    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    result.focus({ preventScroll: true });
    result.scrollIntoView({ block: 'nearest', behavior: reduced ? 'auto' : 'smooth' });
})();
