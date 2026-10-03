using BuliaPortfolio.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BuliaPortfolio.Data;

// Центральный EF Core-контекст: таблицы сайта и таблицы ASP.NET Core Identity.
public class PortfolioDbContext : IdentityDbContext
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