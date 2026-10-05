using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BuliaPortfolio.Pages;

// Демонстрационная страница: показывает, как выглядит лендинг для любого
// бизнеса — услуги, шаги, цены, отзывы, вопросы и форма заявки.
// Форма здесь не сохраняет заявки в базу: это пример, а не рабочий приём заявок.
public class LandingModel : PageModel
{
    [BindProperty]
    public string ClientName { get; set; } = string.Empty;

    [BindProperty]
    public string ClientPhone { get; set; } = string.Empty;

    [BindProperty]
    public string ChosenService { get; set; } = string.Empty;

    [BindProperty]
    public string ClientComment { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true, Name = "sent")]
    public bool IsSent { get; set; }

    public List<ServiceItem> Services { get; } =
    [
        new("Сайт под ключ", "Страницы, форма заявки, панель администратора", "от 25 000 ₽"),
        new("Лендинг", "Один экран под одну услугу или товар", "от 8 000 ₽"),
        new("Доработка сайта", "Исправление ошибок, новые блоки, адаптация под телефон", "от 1 500 ₽"),
        new("Программа для Windows", "Учёт заявок, клиентов или товаров под вашу задачу", "от 12 000 ₽"),
        new("База данных", "Таблицы, связи, отчёты и выгрузки", "от 6 000 ₽"),
        new("Бот для мессенджера", "Заявки и ответы на вопросы прямо в чат", "от 9 000 ₽")
    ];

    public List<PriceItem> Prices { get; } =
    [
        new("Минимальный", "9 900 ₽", "Одна страница или лендинг, форма заявки, запуск за 3 дня",
            ["Один экран", "Форма заявки", "Адаптация под телефон", "Срок 3 дня"]),
        new("Оптимальный", "24 900 ₽", "Многостраничный сайт или лендинг с калькулятором цены",
            ["До 5 страниц", "Калькулятор или расчёт", "Панель администратора", "Срок 10 дней"]),
        new("Под ключ", "по запросу", "Сайт вместе с программой и базой данных",
            ["Всё из предыдущих", "Программа для Windows", "База данных и отчёты", "Срок по договорённости"])
    ];

    public List<SelectListItem> ServiceKinds { get; } =
    [
        new() { Text = "Выберите услугу", Value = "" },
        new() { Text = "Лендинг под одну услугу", Value = "Лендинг под одну услугу" },
        new() { Text = "Сайт под ключ", Value = "Сайт под ключ" },
        new() { Text = "Доработка существующего сайта", Value = "Доработка существующего сайта" },
        new() { Text = "Программа для Windows", Value = "Программа для Windows" },
        new() { Text = "База данных и отчёты", Value = "База данных и отчёты" },
        new() { Text = "Бот для мессенджера", Value = "Бот для мессенджера" },
        new() { Text = "Пока не знаю, нужна консультация", Value = "Пока не знаю, нужна консультация" }
    ];

    public List<FaqItem> Questions { get; } =
    [
        new("Сколько времени делается сайт?", "Лендинг — 3 дня, многостраничный сайт — 10–14 дней, сайт с программой и базой — 20–30 дней. Точный срок называю после обсуждения задачи."),
        new("Что нужно от меня?", "Тексты и пожелания по смыслу. Если нет идей — помогу придумать структуру и тексты сам."),
        new("Работаете по договору?", "Да. Фиксируем объём, срок и стоимость, работаю по предоплате 50%. Остальное — по готовности."),
        new("Что с правками?", "В течение двух недель после сдачи правлю бесплатно. Дальше — за отдельную плату."),
        new("Останется ли мне сайт?", "Да, все файлы и доступы передаю вам. Никакой привязки ко мне, сайт ваш.")
    ];

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        // Примерная форма: проверяем только то, что пришло, и показываем успех.
        if (string.IsNullOrWhiteSpace(ClientName) || string.IsNullOrWhiteSpace(ClientPhone))
        {
            return Page();
        }

        return RedirectToPage(new { sent = true });
    }

    public record ServiceItem(string Title, string Text, string Price);

    public record PriceItem(string Name, string Cost, string Description, List<string> Included);

    public record FaqItem(string Question, string Answer);
}
