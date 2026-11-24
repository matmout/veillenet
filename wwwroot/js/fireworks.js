// Fireworks animation for newsletter subscription success
// Short celebration (about 2.5 seconds), drawn on a transparent canvas

(function() {
    'use strict';

    // Check if fireworks should be triggered
    const triggerFireworks = document.body.getAttribute('data-trigger-fireworks');
    if (triggerFireworks !== 'true') {
        return;
    }

    const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reduce) {
        document.body.removeAttribute('data-trigger-fireworks');
        return;
    }

    const canvas = document.getElementById('fireworks-canvas');
    if (!canvas) {
        return;
    }

    const ctx = canvas.getContext('2d');
    canvas.style.display = 'block';
    canvas.style.opacity = '1';
    canvas.style.transition = 'opacity var(--dur-3) var(--ease-out-quint)';
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;

    const particles = [];
    const fireworks = [];
    // Palette comes from the design tokens (oklch strings are valid canvas colours)
    const rootStyle = getComputedStyle(document.documentElement);
    const tokenColors = ['--token-string', '--token-keyword', '--token-type', '--token-function', '--token-number']
        .map(name => rootStyle.getPropertyValue(name).trim())
        .filter(Boolean);
    const colors = tokenColors.length ? tokenColors : ['currentColor'];
    let raf = 0;
    let running = true;

    class Particle {
        constructor(x, y, color) {
            this.x = x;
            this.y = y;
            this.color = color;
            this.velocity = {
                x: (Math.random() - 0.5) * 6,
                y: (Math.random() - 0.5) * 6
            };
            this.alpha = 1;
            this.decay = Math.random() * 0.015 + 0.015;
            this.radius = Math.random() * 3 + 1;
        }

        update() {
            this.velocity.x *= 0.98;
            this.velocity.y *= 0.98;
            this.velocity.y += 0.1; // gravity
            this.x += this.velocity.x;
            this.y += this.velocity.y;
            this.alpha -= this.decay;
        }

        draw() {
            ctx.save();
            ctx.globalAlpha = this.alpha;
            ctx.beginPath();
            ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2);
            ctx.fillStyle = this.color;
            ctx.fill();
            ctx.restore();
        }
    }

    class Firework {
        constructor(x, y) {
            this.x = x;
            this.y = y;
            this.targetY = Math.random() * canvas.height * 0.4 + 50;
            this.speed = Math.random() * 3 + 6;
            this.angle = Math.PI / 2;
            this.exploded = false;
            this.color = colors[Math.floor(Math.random() * colors.length)];
        }

        update() {
            if (!this.exploded) {
                this.y -= this.speed;
                
                if (this.y <= this.targetY) {
                    this.explode();
                }
            }
        }

        explode() {
            this.exploded = true;
            const particleCount = Math.random() * 50 + 50;
            
            for (let i = 0; i < particleCount; i++) {
                particles.push(new Particle(this.x, this.y, this.color));
            }
        }

        draw() {
            if (!this.exploded) {
                ctx.save();
                ctx.beginPath();
                ctx.arc(this.x, this.y, 3, 0, Math.PI * 2);
                ctx.fillStyle = this.color;
                ctx.fill();
                ctx.restore();
            }
        }
    }

    function animate() {
        if (!running) return;

        // Fade previous frames toward transparent (never paint an opaque layer over the page)
        ctx.globalCompositeOperation = 'destination-out';
        ctx.fillStyle = 'rgba(0, 0, 0, 0.18)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        ctx.globalCompositeOperation = 'source-over';

        // Update and draw particles
        for (let i = particles.length - 1; i >= 0; i--) {
            particles[i].update();
            particles[i].draw();
            
            if (particles[i].alpha <= 0) {
                particles.splice(i, 1);
            }
        }

        // Update and draw fireworks
        for (let i = fireworks.length - 1; i >= 0; i--) {
            fireworks[i].update();
            fireworks[i].draw();
            
            if (fireworks[i].exploded) {
                fireworks.splice(i, 1);
            }
        }

        raf = requestAnimationFrame(animate);
    }

    // Launch fireworks at random intervals
    let fireworkCount = 0;
    const maxFireworks = 8; // Total fireworks during the run
    
    const fireworkInterval = setInterval(() => {
        if (fireworkCount >= maxFireworks) {
            clearInterval(fireworkInterval);
            return;
        }

        const x = Math.random() * canvas.width;
        const y = canvas.height;
        fireworks.push(new Firework(x, y));
        fireworkCount++;
    }, 200); // Launch every 200ms

    // Start animation
    animate();

    function cleanup() {
        running = false;
        cancelAnimationFrame(raf);
        canvas.style.display = 'none';
        canvas.style.opacity = '1';

        particles.length = 0;
        fireworks.length = 0;

        document.body.removeAttribute('data-trigger-fireworks');
    }

    // End the run after 2.5 seconds: CSS fade, then a timeout (not transitionend) so a hidden tab still cleans up
    setTimeout(() => {
        clearInterval(fireworkInterval);
        canvas.style.opacity = '0';
        setTimeout(cleanup, 320);
    }, 2500);

    // Handle window resize
    window.addEventListener('resize', () => {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
    });
})();
