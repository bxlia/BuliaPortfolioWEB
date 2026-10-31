// Квиз на главной: три вопроса, сумма считается сразу.
// Ничего не отправляет, пока человек сам не нажмёт кнопку.

(function () {
    "use strict";

    var quiz = document.getElementById("priceQuiz");
    if (!quiz) {
        return;
    }

    var result = document.getElementById("quizResult");
    var priceEl = document.getElementById("quizPrice");
    var hintEl = document.getElementById("quizHint");

    // Выбранный вариант в каждой группе: { kind: 15000, content: 0, speed: 0 }
    var answers = {};

    var groups = quiz.querySelectorAll(".quiz-options");

    function format(value) {
        return value.toLocaleString("ru-RU") + " ₽";
    }

    function update() {
        var keys = ["kind", "content", "speed"];
        var ready = keys.every(function (key) {
            return typeof answers[key] === "number";
        });

        if (!ready) {
            result.hidden = true;
            return;
        }

        var total = keys.reduce(function (sum, key) {
            return sum + answers[key];
        }, 0);

        priceEl.textContent = format(total);

        // Подсказка зависит от того, что выбрали.
        if (answers.speed === 4000) {
            hintEl.textContent = "Срочный вариант: беру в работу сразу и сдаю за 5 дней.";
        } else if (answers.kind === 5000) {
            hintEl.textContent = "Доработка: сначала посмотрю сайт, потом назову точную сумму — часто выходит меньше.";
        } else {
            hintEl.textContent = "Точную цену скажу после короткого разговора — она может быть ниже.";
        }

        result.hidden = false;
    }

    groups.forEach(function (group) {
        var name = group.getAttribute("data-question");

        group.addEventListener("click", function (event) {
            var button = event.target.closest(".quiz-option");
            if (!button || !group.contains(button)) {
                return;
            }

            group.querySelectorAll(".quiz-option").forEach(function (option) {
                option.classList.remove("is-active");
            });

            button.classList.add("is-active");
            answers[name] = parseInt(button.getAttribute("data-price"), 10) || 0;

            update();
        });
    });
})();
