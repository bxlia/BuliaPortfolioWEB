using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BuliaPortfolio.Services;

// Отправка уведомлений в мессенджер MAX через официальный API.
// Нужен токен бота (Max:Token) и ID чата, куда писать (Max:ChatId).
// Оба значения хранятся в секретах пользователя, а не в коде.
public class MaxBot
{
    private const string BaseUrl = "https://platform-api2.max.ru";

    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<MaxBot> _log;

    public MaxBot(IConfiguration config, IHttpClientFactory factory, ILogger<MaxBot> log)
    {
        _config = config;
        _factory = factory;
        _log = log;
    }

    public string? Token => _config["Max:Token"];

    public string? ChatId => _config["Max:ChatId"];

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Token) && !string.IsNullOrWhiteSpace(ChatId);

    // Отправляет текст в чат. Возвращает false, если отправка не вышла:
    // это не должно ломать форму, заявка всё равно сохранена в базе.
    public async Task<bool> SendAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(Token) || string.IsNullOrWhiteSpace(ChatId))
        {
            _log.LogWarning("MAX: токен или ID чата не заданы, уведомление не отправлено.");
            return false;
        }

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{BaseUrl}/messages?chat_id={ChatId}");

            // Токен передаём в заголовке, не в адресе.
            request.Headers.TryAddWithoutValidation("Authorization", Token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(
                JsonSerializer.Serialize(new { text }),
                Encoding.UTF8,
                "application/json");

            var client = _factory.CreateClient("max");
            using var response = await client.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _log.LogInformation("MAX: уведомление отправлено.");
                return true;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _log.LogError("MAX: сервер вернул {0}. {1}", (int)response.StatusCode, body);
            return false;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "MAX: не удалось отправить уведомление.");
            return false;
        }
    }

    // Одноразовый поиск ID чата. Способ один: написать боту любое сообщение,
    // оно придёт в списке непрочитанных событий, и мы вытащим оттуда ID чата.
    public static async Task<string?> DiscoverChatIdAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var log = scope.ServiceProvider.GetRequiredService<ILogger<MaxBot>>();

        var token = config["Max:Token"];

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
                $"{BaseUrl}/updates?timeout=0&limit=100");

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

    // В списке событий чат лежит в message.chat.id.
    // Разбираем осторожно: структуру событий MAX меняет.
    private static string? FindChatId(string json, ILogger log)
    {
        try
        {
            using var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty("updates", out var updates)
                || updates.ValueKind != JsonValueKind.Array)
            {
                log.LogWarning("MAX: в ответе нет списка событий.");
                return null;
            }

            foreach (var update in updates.EnumerateArray())
            {
                if (!update.TryGetProperty("message", out var message))
                {
                    continue;
                }

                if (message.TryGetProperty("chat", out var chat)
                    && chat.TryGetProperty("id", out var id)
                    && id.ValueKind == JsonValueKind.Number)
                {
                    var chatId = id.GetInt64();
                    log.LogWarning("MAX: найден ID чата {0}. Запишите его в Max:ChatId.", chatId);
                    return chatId.ToString();
                }
            }

            log.LogWarning("MAX: событий с чатом нет. Напишите боту любое сообщение в MAX и запустите сайт ещё раз.");
            return null;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "MAX: не удалось разобрать ответ.");
            return null;
        }
    }
}