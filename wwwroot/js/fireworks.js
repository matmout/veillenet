// Fireworks animation for newsletter subscription success
// Triggers a 4-second fireworks celebration

(function() {
    'use strict';

    // Check if fireworks should be triggered
    const triggerFireworks = document.body.getAttribute('data-trigger-fireworks');
    if (triggerFireworks !== 'true') {
        return;
    }

    const canvas = document.getElementById('fireworks-canvas');
    if (!canvas) {
        return;
    }

    const ctx = canvas.getContext('2d');
    canvas.style.display = 'block';
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;

    const particles = [];
    const fireworks = [];
    const colors = ['#ff0844', '#ffb700', '#00e5ff', '#00ff88', '#b300ff', '#ff006e'];

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
        ctx.fillStyle = 'rgba(0, 0, 0, 0.1)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

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

        requestAnimationFrame(animate);
    }

    // Launch fireworks at random intervals
    let fireworkCount = 0;
    const maxFireworks = 20; // Total fireworks during 4 seconds
    
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

    // Clean up after 4 seconds
    setTimeout(() => {
        clearInterval(fireworkInterval);
        
        // Fade out canvas
        let fadeAlpha = 1;
        const fadeInterval = setInterval(() => {
            fadeAlpha -= 0.05;
            canvas.style.opacity = fadeAlpha;
            
            if (fadeAlpha <= 0) {
                clearInterval(fadeInterval);
                canvas.style.display = 'none';
                canvas.style.opacity = 1;
                
                // Clear arrays
                particles.length = 0;
                fireworks.length = 0;
                
                // Remove trigger attribute
                document.body.removeAttribute('data-trigger-fireworks');
            }
        }, 50);
    }, 4000);

    // Handle window resize
    window.addEventListener('resize', () => {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
    });
})();
