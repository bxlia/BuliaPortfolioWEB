using System.Collections.Concurrent;

namespace BuliaPortfolio.Services;

// Простая защита формы от спам-ботов. Работает без внешних сервисов:
//  1. Проверяем, что форму заполняли не мгновенно — боты отправляют сразу.
//  2. Ограничиваем число отправок с одного адреса.
// Адреса берём из X-Forwarded-For, потому что сайт часто открывают через
// туннель и тогда реальный адрес посетителя пропадает.
public static class SpamGuard
{
    // Не больше трёх отправок с одного адреса в час.
    private const int MaxPerHour = 3;

    private static readonly TimeSpan Window = TimeSpan.FromHours(1);

    // Меньше трёх секунд на заполнение — считаем ботом.
    private static readonly long MinFillTicks = TimeSpan.TicksPerSecond * 3;

    // Защита от роста памяти: если адресов стало слишком много,
    // начинаем с чистого листа.
    private const int MaxTrackedKeys = 5000;

    private static readonly ConcurrentDictionary<string, Queue<long>> Sent = new();

    // Форму заполнили подозрительно быстро.
    public static bool FilledTooFast(long stamp)
    {
        // Поле не пришло — это не повод отказывать, а повод не наказывать.
        if (stamp <= 0)
        {
            return false;
        }

        var elapsed = DateTime.UtcNow.Ticks - stamp;

        // Если часы на компьютере сбиты и время ушло в будущее — пропускаем.
        if (elapsed < 0)
        {
            return false;
        }

        return elapsed < MinFillTicks;
    }

    // С этого адреса уже отправлено слишком много заявок.
    public static bool TooManyFrom(string? clientKey)
    {
        if (string.IsNullOrWhiteSpace(clientKey))
        {
            return false;
        }

        if (Sent.Count > MaxTrackedKeys)
        {
            Sent.Clear();
        }

        var now = DateTime.UtcNow.Ticks;
        var queue = Sent.GetOrAdd(clientKey, _ => new Queue<long>());

        lock (queue)
        {
            while (queue.Count > 0 && now - queue.Peek() > Window.Ticks)
            {
                queue.Dequeue();
            }

            if (queue.Count >= MaxPerHour)
            {
                return true;
            }

            queue.Enqueue(now);

            return false;
        }
    }

    // Адрес посетителя: сначала заголовок от туннеля, потом подключение.
    public static string? GetClientKey(HttpContext http)
    {
        var forwarded = http.Request.Headers["X-Forwarded-For"].ToString();

        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            // Может быть список через запятую — нужен первый.
            var first = forwarded.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (first.Length > 0)
            {
                return first[0];
            }
        }

        return http.Connection.RemoteIpAddress?.ToString();
    }
}