// Общий скрипт демо-сайтов: появление блоков, счётчики, тень у шапки.
// Без библиотек. Если что-то не поддерживается — страница просто остаётся видимой.

(function () {
    "use strict";

    var body = document.body;
    var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    // ---------- Появление при прокрутке ----------
    var items = document.querySelectorAll(".d-reveal");

    function showAll() {
        for (var i = 0; i < items.length; i++) {
            items[i].classList.add("is-in");
        }
    }

    if (!("IntersectionObserver" in window) || reduceMotion) {
        showAll();
    } else {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) {
                    return;
                }

                entry.target.classList.add("is-in");
                startCounters(entry.target);
                observer.unobserve(entry.target);
            });
        }, { threshold: 0.14, rootMargin: "0px 0px -40px 0px" });

        items.forEach(function (item) {
            observer.observe(item);
        });
    }

    // ---------- Счётчики ----------
    function animateCounter(node) {
        var target = parseInt(node.getAttribute("data-count"), 10) || 0;
        var suffix = node.getAttribute("data-suffix") || "";
        var duration = 900;
        var start = null;

        function step(now) {
            if (start === null) {
                start = now;
            }

            var progress = Math.min(1, (now - start) / duration);
            var eased = 1 - Math.pow(1 - progress, 3);
            node.textContent = Math.round(target * eased).toLocaleString("ru-RU") + suffix;

            if (progress < 1) {
                window.requestAnimationFrame(step);
            }
        }

        window.requestAnimationFrame(step);
    }

    function startCounters(scope) {
        var counters = scope.querySelectorAll("[data-count]");

        for (var i = 0; i < counters.length; i++) {
            if (counters[i].getAttribute("data-done") === "1") {
                continue;
            }

            counters[i].setAttribute("data-done", "1");

            if (reduceMotion) {
                var target = parseInt(counters[i].getAttribute("data-count"), 10) || 0;
                counters[i].textContent = target.toLocaleString("ru-RU") + (counters[i].getAttribute("data-suffix") || "");
            } else {
                animateCounter(counters[i]);
            }
        }
    }

    // ---------- Тень у шапки после прокрутки ----------
    var lastState = false;

    function onScroll() {
        var scrolled = window.scrollY > 12;

        if (scrolled !== lastState) {
            lastState = scrolled;
            body.classList.toggle("is-scrolled", scrolled);
        }
    }

    window.addEventListener("scroll", onScroll, { passive: true });
    onScroll();

    // Счётчики в первом экране видны сразу — запускаем их без ожидания прокрутки.
    startCounters(document);
})();
