namespace BuliaPortfolio.Models;

// Модель таблицы dbo.Skills из базы BuliaPortfolioDb.
public class Skill
{
	public int Id { get; set; }

	public string Name { get; set; } = string.Empty;

	public string? Category { get; set; }

	public int LevelPercent { get; set; }

	public string? IconUrl { get; set; }

	public int DisplayOrder { get; set; }
}