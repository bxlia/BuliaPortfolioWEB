using BuliaPortfolio.Models;
using Microsoft.EntityFrameworkCore;

namespace BuliaPortfolio.Data;

// Центральный класс EF Core: связывает C#-модели с таблицами SQL Server.
public class PortfolioDbContext : DbContext
{
	public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
		: base(options)
	{
	}

	public DbSet<Skill> Skills { get; set; }

	public DbSet<Project> Projects { get; set; }

	public DbSet<ContactRequest> ContactRequests { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Skill>().ToTable("Skills");
		modelBuilder.Entity<Project>().ToTable("Projects");
		modelBuilder.Entity<ContactRequest>().ToTable("ContactRequests");
	}
}