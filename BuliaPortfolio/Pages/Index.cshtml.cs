using BuliaPortfolio.Data;
using BuliaPortfolio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BuliaPortfolio.Pages;

// Модель главной страницы: получает опубликованные проекты из SQL Server.
public class IndexModel : PageModel
{
	private readonly PortfolioDbContext _context;

	public IndexModel(PortfolioDbContext context)
	{
		_context = context;
	}

	public List<Project> Projects { get; private set; } = [];

	[BindProperty]
	public string ClientName { get; set; } = "";

	[BindProperty]
	public string ClientContact { get; set; } = "";

	[BindProperty]
	public string ClientMessage { get; set; } = "";

	public bool IsRequestSent { get; set; }

	public async Task OnGetAsync()
	{
		Projects = await _context.Projects
			.Where(project => project.IsPublished)
			.OrderBy(project => project.DisplayOrder)
			.ToListAsync();
	}

	public void OnPost()
	{
		IsRequestSent = true;
	}
}