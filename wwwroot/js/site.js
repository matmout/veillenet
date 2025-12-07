// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
(function () {
    // Interactive tilt on hover based on mouse position
    const MAX_ROTATE_DEG = 3; // limit rotation
    const MAX_TRANSLATE_PX = 6; // slight parallax

    function handleMouseMove(e) {
        const el = e.currentTarget;
        const rect = el.getBoundingClientRect();
        const x = e.clientX - rect.left; // mouse x within element
        const y = e.clientY - rect.top;  // mouse y within element
        const cx = rect.width / 2;
        const cy = rect.height / 2;

        const nx = (x - cx) / cx; // -1..1
        const ny = (y - cy) / cy; // -1..1

        const rotateY = -nx * MAX_ROTATE_DEG; // tilt left/right
        const rotateX = ny * MAX_ROTATE_DEG;  // tilt up/down
        const translateX = nx * MAX_TRANSLATE_PX;
        const translateY = ny * MAX_TRANSLATE_PX;

        el.style.transform = `translate(${translateX}px, ${translateY}px) rotateX(${rotateX}deg) rotateY(${rotateY}deg)`;
    }

    function handleMouseLeave(e) {
        const el = e.currentTarget;
        el.style.transform = ""; // reset to CSS default (with transition)
    }

    function initHoverTilt() {
        document.querySelectorAll('.hover-tilt').forEach(el => {
            // Ensure GPU acceleration
            el.style.willChange = 'transform';
            el.addEventListener('mousemove', handleMouseMove);
            el.addEventListener('mouseleave', handleMouseLeave);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initHoverTilt);
    } else {
        initHoverTilt();
    }
})();

(function () {
  const el = document.getElementById('output-stream');
  if (!el) return;

  const delay = (ms) => new Promise(r => setTimeout(r, ms));
  const print = async (line, cls = '') => {
    const div = document.createElement('div');
    if (cls) div.className = cls;
    div.textContent = line;
    el.appendChild(div);
    el.scrollTop = el.scrollHeight;
    await delay(60 + Math.random() * 120);
  };

  const timestamp = () => new Date().toISOString().replace('T', ' ').replace('Z', '');

  const scenario = async () => {
    el.innerHTML = '';
    await print(`[${timestamp()}] dotnet --info`, 'dim');
    await print(' .NET SDK: 10.0.x (linux-x64)');
    await print(' ASP.NET Core: 10.0.x');
    await print('');

    await print(`[${timestamp()}] dotnet new web -n ContainSharp.Web`, 'dim');
    await print(' Restoring packages for ContainSharp.Web...');
    await print(' Restore completed in 0.8 sec', 'ok');

    await print(`[${timestamp()}] dotnet build`, 'dim');
    await print(' Building ContainSharp.Web -> /src/bin/Debug/net10.0/ContainSharp.Web.dll');
    await print(' Build succeeded', 'ok');

    await print(`[${timestamp()}] dotnet run`, 'dim');
    await print(' Now listening on: http://0.0.0.0:8080', 'ok');
    await print(' Application started. Press Ctrl+C to shut down.', 'ok');
    await print('');

    await print(`[${timestamp()}] Initializing services...`, 'dim');
    await print(' Connecting to MemoryCache: OK', 'ok');
    await print(' Connecting to Redis cache... timeout, retrying', 'warn');
    await print(' Connecting to Redis cache: OK', 'ok');
    await print(' HTTP client factory ready', 'ok');
    await print('');

    await print(`[${timestamp()}] Calling external APIs`, 'dim');
    await print(' GET https://devblogs.microsoft.com/dotnet/feed/ 200 (124ms)', 'ok');
    await print(' GET https://github.blog/feed/ 200 (98ms)', 'ok');
    await print(' GET https://community.devexpress.com/blogs/winforms/rss.aspx 200 (156ms)', 'ok');
    await print(' GET https://api.github.com/search/repositories?q=language:csharp&sort=stars 200 (312ms)', 'ok');
    await print('');

    await print(`[${timestamp()}] Background jobs`, 'dim');
    await print(' Cache refresh scheduled in 60 min', 'dim');
    await print(' GitHub trending refresh scheduled in 6 hours', 'dim');
    await delay(800);

    await print(`[${timestamp()}] Shutting down...`, 'warn');
    await print(' Disposing HttpClientFactory', 'dim');
    await print(' Stopping web server', 'dim');
    await print('');
    await print(' --- Restarting ---', 'warn');
  };

  (async function loop() {
    while (true) {
      try { await scenario(); } catch { }
      await delay(900);
    }
  })();
})();
