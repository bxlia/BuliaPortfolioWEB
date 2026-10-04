using System.ComponentModel.DataAnnotations;

namespace BuliaPortfolio.Models;

// Модель таблицы dbo.Skills из базы BuliaPortfolioDb.
// Длины строк соответствуют реальным колонкам, созданным в SQL Server,
// иначе EF Core ожидал бы nvarchar(450) и сгенерировал бы лишний ALTER.
public class Skill
{
	public int Id { get; set; }

	[MaxLength(200)]
	public string Name { get; set; } = string.Empty;

	[MaxLength(200)]
	public string? Category { get; set; }

	[Range(0, 100)]
	public int LevelPercent { get; set; }

	[MaxLength(1000)]
	public string? IconUrl { get; set; }

	public int DisplayOrder { get; set; }
}
