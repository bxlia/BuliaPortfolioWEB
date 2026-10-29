using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BuliaPortfolio.Services;

// Отправка уведомлений в мессенджер MAX через официальный API.
// Нужен токен бота (Max:Token) и ID чата, куда писать (Max:ChatId).
// Оба значения хранятся в секретах пользователя, а не в коде.
public class MaxBot
{
    // Официальный адрес API и запасной.
    // platform-api2.max.ru подписан сертификатом Минцифры, а его корневой
    // сертификат есть не на каждом хостинге: тогда рукопожатие падает с
    // "The SSL connection could not be established". botapi.max.ru —
    // тот же самый API, но с обычным сертификатом HARICA, который доверяют все.
    private static readonly string[] BaseUrls =
    {
        "https://platform-api2.max.ru",
        "https://botapi.max.ru"
    };

    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<MaxBot> _log;

    public MaxBot(IConfiguration config, IHttpClientFactory factory, ILogger<MaxBot> log)
    {
        _config = config;
        _factory = factory;
        _log = log;
    }

    // Trim: в переменных хостинга часто остаётся пробел или перевод строки,
    // из-за этого MAX отвечает 401.
    public string? Token => _config["Max:Token"]?.Trim();

    public string? ChatId => _config["Max:ChatId"]?.Trim();

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Token) && !string.IsNullOrWhiteSpace(ChatId);

    // Отправляет текст в чат. Возвращает false, если отправка не вышла:
    // это не должно ломать форму, заявка всё равно сохранена в базе.
    public async Task<bool> SendAsync(string text, CancellationToken cancellationToken = default)
    {
        var (ok, details) = await SendCoreAsync(text, cancellationToken);

        if (ok)
        {
            _log.LogInformation("MAX: уведомление отправлено.");
        }
        else
        {
            _log.LogError("MAX: {Details}", details);
        }

        return ok;
    }

    // Проверка связи из админки. Возвращает текст, который видно на странице,
    // поэтому для диагностики не нужно искать строки в логах.
    public async Task<string> TestAsync(CancellationToken cancellationToken = default)
    {
        var (ok, details) = await SendCoreAsync(
            "Тест с сайта: проверка связи с MAX.", cancellationToken);

        return ok ? "Успешно. " + details : details;
    }

    private async Task<(bool Ok, string Details)> SendCoreAsync(
        string text,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Token) || string.IsNullOrWhiteSpace(ChatId))
        {
            return (false, "Токен или ID чата не заданы.");
        }

        // Пробуем адреса по очереди. Отдельный try на каждый: если упало
        // рукопожатие на первом, второй адрес всё равно даст отправить.
        var errors = new List<string>();

        foreach (var baseUrl in BaseUrls)
        {
            try
            {
                var (ok, details) = await SendToAsync(baseUrl, text, cancellationToken);

                if (ok)
                {
                    return (true, details);
                }

                errors.Add($"{baseUrl}: {details}");
            }
            catch (Exception ex)
            {
                errors.Add($"{baseUrl}: {Describe(ex)}");
            }
        }

        return (false, "Не удалось отправить. " + string.Join(" | ", errors));
    }

    private async Task<(bool Ok, string Details)> SendToAsync(
        string baseUrl,
        string text,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl}/messages?chat_id={ChatId}");

        // Токен передаём в заголовке, не в адресе.
        request.Headers.TryAddWithoutValidation("Authorization", Token);
        request.Headers.TryAddWithoutValidation("User-Agent", "BuliaPortfolio/1.0");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(
            JsonSerializer.Serialize(new { text }),
            Encoding.UTF8,
            "application/json");

        var client = _factory.CreateClient("max");
        using var response = await client.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return (true, $"Сообщение отправлено ({new Uri(baseUrl).Host}).");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return (false, $"сервер вернул {(int)response.StatusCode}. {body}");
    }

    // Внутренняя ошибка объясняет причину: именно там лежит текст про
    // сертификат, таймаут или DNS. Без неё видно только общую фразу.
    private static string Describe(Exception ex)
    {
        var text = $"{ex.GetType().Name}: {ex.Message}";

        for (var inner = ex.InnerException; inner is not null; inner = inner.InnerException)
        {
            text += $" ← {inner.GetType().Name}: {inner.Message}";
        }

        return text;
    }

    // Одноразовый поиск ID чата. Способ один: написать боту любое сообщение,
    // оно придёт в списке непрочитанных событий, и мы вытащим оттуда ID чата.
    public static async Task<string?> DiscoverChatIdAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var log = scope.ServiceProvider.GetRequiredService<ILogger<MaxBot>>();

        var token = config["Max:Token"]?.Trim();

        if (string.IsNullOrWhiteSpace(token))
        {
            log.LogWarning("MAX: токен не задан, поиск чата пропущен.");
            return null;
        }

        if (!string.IsNullOrWhiteSpace(config["Max:ChatId"]))
        {
            return config["Max:ChatId"];
        }

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{BaseUrls[0]}/updates?timeout=0&limit=100");

            request.Headers.TryAddWithoutValidation("Authorization", token);

            var client = factory.CreateClient("max");
            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                log.LogError("MAX: поиск чата вернул {0}.", (int)response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            return FindChatId(json, log);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "MAX: поиск чата не удался.");
            return null;
        }
    }

    // В списке событий номер чата лежит глубоко внутри: у сообщения
    // это recipient.chat_id, у события bot_started — другой путь.
    // Структуру MAX меняет, поэтому ищем рекурсивно первое же chat_id.
    private static string? FindChatId(string json, ILogger log)
    {
        try
        {
            using var document = JsonDocument.Parse(json);

            var chatId = FindFirstChatId(document.RootElement);

            if (chatId is not null)
            {
                log.LogWarning("MAX: найден ID чата {0}.", chatId);
                return chatId;
            }

            log.LogWarning(
                "MAX: событий с чатом нет. Напишите боту любое сообщение в MAX и запустите сайт ещё раз.");
            return null;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "MAX: не удалось разобрать ответ.");
            return null;
        }
    }

    private static string? FindFirstChatId(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    // Самый частый вариант: recipient: { chat_id: 123 }
                    if (property.NameEquals("chat_id")
                        && property.Value.ValueKind == JsonValueKind.Number)
                    {
                        return property.Value.GetInt64().ToString();
                    }

                    // Вариант с объектом чата: chat: { id: 123 }
                    if (property.NameEquals("id")
                        && element.TryGetProperty("type", out _)
                        && property.Value.ValueKind == JsonValueKind.Number)
                    {
                        return property.Value.GetInt64().ToString();
                    }

                    var nested = FindFirstChatId(property.Value);

                    if (nested is not null)
                    {
                        return nested;
                    }
                }

                return null;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    var nested = FindFirstChatId(item);

                    if (nested is not null)
                    {
                        return nested;
                    }
                }

                return null;

            default:
                return null;
        }
    }
}