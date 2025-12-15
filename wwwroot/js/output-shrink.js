// Progressive shrinking of Output Window on scroll
(function() {
    const outputWindow = document.querySelector('.vs-output');
    if (!outputWindow) return;

    const initialHeight = 260; // Height in pixels from CSS
    const minHeight = 0; // Fully hidden
    const shrinkStart = 100; // Start shrinking after 100px scroll
    const shrinkEnd = 500; // Fully hidden at 500px scroll

    function updateOutputHeight() {
        const scrollY = window.scrollY || window.pageYOffset;

        if (scrollY <= shrinkStart) {
            // Above shrink start: full height
            outputWindow.style.maxHeight = `${initialHeight}px`;
            outputWindow.style.opacity = '1';
            outputWindow.style.marginBottom = '1.5rem';
        } else if (scrollY >= shrinkEnd) {
            // Below shrink end: fully hidden
            outputWindow.style.maxHeight = '0px';
            outputWindow.style.opacity = '0';
            outputWindow.style.marginBottom = '0';
        } else {
            // In between: progressive shrink
            const progress = (scrollY - shrinkStart) / (shrinkEnd - shrinkStart);
            const currentHeight = initialHeight * (1 - progress);
            const currentOpacity = 1 - progress;
            
            outputWindow.style.maxHeight = `${currentHeight}px`;
            outputWindow.style.opacity = `${currentOpacity}`;
            outputWindow.style.marginBottom = `${1.5 * (1 - progress)}rem`;
        }
    }

    // Add smooth transition
    outputWindow.style.transition = 'max-height 0.3s ease, opacity 0.3s ease, margin-bottom 0.3s ease';
    outputWindow.style.overflow = 'hidden';

    // Listen to scroll events with throttling for performance
    let ticking = false;
    window.addEventListener('scroll', function() {
        if (!ticking) {
            window.requestAnimationFrame(function() {
                updateOutputHeight();
                ticking = false;
            });
            ticking = true;
        }
    });

    // Initial call
    updateOutputHeight();
})();
