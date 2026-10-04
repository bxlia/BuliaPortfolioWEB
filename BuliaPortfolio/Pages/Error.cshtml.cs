using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BuliaPortfolio.Pages;

// Страница ошибок: 404, 403, 500 и необработанные исключения.
// Нормальный код ответа (404, 500) сохраняется — для поисковиков и клиентов это важно.
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; private set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string Title { get; private set; } = "Что-то пошло не так";

    public string Message { get; private set; } =
        "Возникла ошибка при обработке запроса. Попробуйте обновить страницу.";

    private readonly ILogger<ErrorModel> _logger;

    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    public void OnGet(int? code)
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        (Title, Message) = code switch
        {
            404 => ("Страница не найдена",
                "Такой страницы нет. Возможно, ссылка устарела или в адресе опечатка."),
            403 => ("Доступ запрещён",
                "У вас нет прав, чтобы открыть эту страницу."),
            401 => ("Нужно войти",
                "Эта страница доступна только после входа в аккаунт."),
            400 => ("Неверный запрос",
                "Сервер не смог обработать запрос. Проверьте адрес страницы."),
            >= 500 => ("Ошибка сервера",
                "Внутренняя ошибка. Мы уже знаем о ней и скоро поправим."),
            _ => ("Что-то пошло не так",
                "Возникла ошибка при обработке запроса. Попробуйте обновить страницу.")
        };

        if (code >= 500)
        {
            _logger.LogError("Ошибка {StatusCode}, RequestId {RequestId}",
                code, RequestId);
        }
        else
        {
            _logger.LogWarning("Клиентский запрос отклонён: {StatusCode}", code);
        }
    }
}
