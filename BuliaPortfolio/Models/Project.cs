using System.ComponentModel.DataAnnotations;

namespace BuliaPortfolio.Models;

// Модель таблицы dbo.Projects из базы BuliaPortfolioDb.
// Длины строк соответствуют реальным колонкам, созданным в SQL Server.
public class Project
{
	public int Id { get; set; }

	[MaxLength(300)]
	public string Title { get; set; } = string.Empty;

	[MaxLength(1000)]
	public string ShortDescription { get; set; } = string.Empty;

	public string? FullDescription { get; set; }

	[MaxLength(1000)]
	public string? ImageUrl { get; set; }

	[MaxLength(1000)]
	public string? ProjectUrl { get; set; }

	[MaxLength(1000)]
	public string? GitHubUrl { get; set; }

	[MaxLength(1000)]
	public string? Technologies { get; set; }

	public bool IsPublished { get; set; }

	public int DisplayOrder { get; set; }

	public DateTime CreatedAt { get; set; }
}
