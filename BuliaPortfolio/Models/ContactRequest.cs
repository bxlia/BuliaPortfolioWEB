using System.ComponentModel.DataAnnotations;

namespace BuliaPortfolio.Models;

// Модель таблицы dbo.ContactRequests из базы BuliaPortfolioDb.
// Длины строк соответствуют реальным колонкам, созданным в SQL Server:
// ClientName nvarchar(300), Email nvarchar(510), Phone nvarchar(100).
public class ContactRequest
{
	public int Id { get; set; }

	[MaxLength(300)]
	public string ClientName { get; set; } = string.Empty;

	[MaxLength(510)]
	public string Email { get; set; } = string.Empty;

	[MaxLength(100)]
	public string? Phone { get; set; }

	public string Message { get; set; } = string.Empty;

	public bool IsProcessed { get; set; }

	public DateTime CreatedAt { get; set; }
}
