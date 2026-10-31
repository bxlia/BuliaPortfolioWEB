// Плавное появление цифр: <span data-count="12" data-suffix="+">12+</span>.
// Отдельный файл, чтобы не трогать основной site.js.

(function () {
    "use strict";

    var counters = document.querySelectorAll("[data-count]");

    if (counters.length === 0) {
        return;
    }

    var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    function format(node, value) {
        return value.toLocaleString("ru-RU") + (node.getAttribute("data-suffix") || "");
    }

    function run(node) {
        var target = parseInt(node.getAttribute("data-count"), 10) || 0;

        if (reduceMotion || !window.requestAnimationFrame) {
            node.textContent = format(node, target);
            return;
        }

        var start = null;

        function step(now) {
            if (start === null) {
                start = now;
            }

            var progress = Math.min(1, (now - start) / 900);
            var eased = 1 - Math.pow(1 - progress, 3);
            node.textContent = format(node, Math.round(target * eased));

            if (progress < 1) {
                window.requestAnimationFrame(step);
            }
        }

        window.requestAnimationFrame(step);
    }

    if (!("IntersectionObserver" in window)) {
        counters.forEach(run);
        return;
    }

    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (!entry.isIntersecting) {
                return;
            }

            run(entry.target);
            observer.unobserve(entry.target);
        });
    }, { threshold: 0.5 });

    counters.forEach(function (node) {
        observer.observe(node);
    });
})();
