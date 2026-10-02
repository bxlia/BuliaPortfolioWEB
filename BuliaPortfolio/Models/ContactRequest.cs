namespace BuliaPortfolio.Models;

// Модель заявок клиентов из таблицы dbo.ContactRequests.
public class ContactRequest
{
	public int Id { get; set; }

	public string ClientName { get; set; } = string.Empty;

	public string Email { get; set; } = string.Empty;

	public string? Phone { get; set; }

	public string Message { get; set; } = string.Empty;

	public bool IsProcessed { get; set; }

	public DateTime CreatedAt { get; set; }
}