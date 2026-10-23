// ==========================================================
// BuliaPortfolio — небольшие улучшения интерфейса.
// Всё необязательное: если скрипт не запустился, сайт работает.
// ==========================================================

(function () {
	"use strict";

	document.documentElement.classList.add("js");

	// ---------- Появление блоков при прокрутке ----------
	var revealItems = document.querySelectorAll(".reveal");

	if ("IntersectionObserver" in window && revealItems.length > 0) {
		var revealObserver = new IntersectionObserver(function (entries) {
			entries.forEach(function (entry) {
				if (entry.isIntersecting) {
					entry.target.classList.add("is-visible");
					revealObserver.unobserve(entry.target);
				}
			});
		}, { threshold: 0.12, rootMargin: "0px 0px -40px 0px" });

		revealItems.forEach(function (item) {
			revealObserver.observe(item);
		});
	} else {
		revealItems.forEach(function (item) {
			item.classList.add("is-visible");
		});
	}

	// ---------- Полоса прогресса чтения ----------
	var progress = document.querySelector(".scroll-progress");

	if (progress) {
		var updateProgress = function () {
			var height = document.documentElement.scrollHeight - window.innerHeight;
			var done = height > 0 ? (window.scrollY / height) * 100 : 0;

			progress.style.width = Math.min(100, Math.max(0, done)) + "%";
		};

		window.addEventListener("scroll", updateProgress, { passive: true });
		window.addEventListener("resize", updateProgress);
		updateProgress();
	}

	// ---------- Подсветка текущего раздела в меню ----------
	var navLinks = document.querySelectorAll(".site-nav .nav-link");
	var sections = document.querySelectorAll("section[id]");

	if (navLinks.length > 0 && sections.length > 0 && "IntersectionObserver" in window) {
		var sectionObserver = new IntersectionObserver(function (entries) {
			entries.forEach(function (entry) {
				if (!entry.isIntersecting) {
					return;
				}

				navLinks.forEach(function (link) {
					var hash = (link.getAttribute("href") || "").replace(/^\//, "");

					link.classList.toggle(
						"active",
						hash === "#" + entry.target.id
					);
				});
			});
		}, { rootMargin: "-45% 0px -50% 0px" });

		sections.forEach(function (section) {
			sectionObserver.observe(section);
		});
	}

	// ---------- Копирование контакта по клику ----------
	var copyTargets = document.querySelectorAll("[data-copy]");

	copyTargets.forEach(function (element) {
		element.style.cursor = "pointer";

		element.addEventListener("click", function () {
			var text = element.getAttribute("data-copy");

			if (!navigator.clipboard) {
				return;
			}

			navigator.clipboard.writeText(text).then(function () {
				var original = element.getAttribute("title") || "";
				element.setAttribute("title", "Скопировано");
				element.classList.add("copied");

				setTimeout(function () {
					element.setAttribute("title", original);
					element.classList.remove("copied");
				}, 1400);
			});
		});
	});
})();