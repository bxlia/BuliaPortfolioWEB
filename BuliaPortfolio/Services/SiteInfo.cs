namespace BuliaPortfolio.Services;

// Контакты в одном месте: меняешь здесь — меняется на всех страницах.
// Телефон на сайте не публикуем, связь только через мессенджер и почту.
public static class SiteInfo
{
    // Ник в Telegram без собаки.
    public const string Telegram = "Bulia4";

    public const string Email = "bulia44444@gmail.com";

    public static string TelegramUrl(string text = "") =>
        $"https://t.me/{Telegram}" + (string.IsNullOrWhiteSpace(text)
            ? string.Empty
            : "?text=" + Uri.EscapeDataString(text));
}
