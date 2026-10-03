using BuliaPortfolio.Data;
using BuliaPortfolio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BuliaPortfolio.Pages;

// Модель главной страницы: загружает проекты и навыки, сохраняет заявки в SQL Server.
public class IndexModel : PageModel
{
	private readonly PortfolioDbContext _context;

	public IndexModel(PortfolioDbContext context)
	{
		_context = context;
	}

	public List<Project> Projects { get; private set; } = [];

	public List<Skill> Skills { get; private set; } = [];

	[BindProperty]
	public string ClientName { get; set; } = "";

	[BindProperty]
	public string ClientEmail { get; set; } = "";

	[BindProperty]
	public string ClientMessage { get; set; } = "";

	[BindProperty(SupportsGet = true, Name = "sent")]
	public bool IsRequestSent { get; set; }

	public async Task OnGetAsync()
	{
		await LoadPageDataAsync();
	}

	public async Task<IActionResult> OnPostAsync()
	{
		if (string.IsNullOrWhiteSpace(ClientName) ||
			string.IsNullOrWhiteSpace(ClientEmail) ||
			string.IsNullOrWhiteSpace(ClientMessage))
		{
			await LoadPageDataAsync();

			return Page();
		}

		var request = new ContactRequest
		{
			ClientName = ClientName,
			Email = ClientEmail,
			Message = ClientMessage,
			IsProcessed = false,
			CreatedAt = DateTime.UtcNow
		};

		_context.ContactRequests.Add(request);
		await _context.SaveChangesAsync();

		return RedirectToPage(new { sent = true });
	}

	private async Task LoadPageDataAsync()
	{
		Projects = await _context.Projects
			.Where(project => project.IsPublished)
			.OrderBy(project => project.DisplayOrder)
			.ToListAsync();

		Skills = await _context.Skills
			.OrderBy(skill => skill.DisplayOrder)
			.ToListAsync();
	}
}