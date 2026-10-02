namespace BuliaPortfolio.Models;

// Модель таблицы dbo.Projects из базы BuliaPortfolioDb.
public class Project
{
	public int Id { get; set; }

	public string Title { get; set; } = string.Empty;

	public string ShortDescription { get; set; } = string.Empty;

	public string? FullDescription { get; set; }

	public string? ImageUrl { get; set; }

	public string? ProjectUrl { get; set; }

	public string? GitHubUrl { get; set; }

	public string? Technologies { get; set; }

	public bool IsPublished { get; set; }

	public int DisplayOrder { get; set; }

	public DateTime CreatedAt { get; set; }
}