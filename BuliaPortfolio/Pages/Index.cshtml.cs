using BuliaPortfolio.Data;
using BuliaPortfolio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BuliaPortfolio.Pages;

// Главная страница: показывает проекты и навыки, принимает заявку в SQL Server.
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
	public ContactRequestInput ClientForm { get; set; } = new();

	[BindProperty(SupportsGet = true, Name = "sent")]
	public bool IsRequestSent { get; set; }

	public async Task OnGetAsync()
	{
		await LoadPageDataAsync();
	}

	public async Task<IActionResult> OnPostAsync()
	{
		await LoadPageDataAsync();

		// Honeypot: скрытое поле заполнил бот — показываем успех, но не пишем в базу.
		if (!string.IsNullOrWhiteSpace(ClientForm.Website))
		{
			return RedirectToPage(new { sent = true });
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		_context.ContactRequests.Add(ClientForm.ToEntity());

		await _context.SaveChangesAsync();

		return RedirectToPage(new { sent = true });
	}

	private async Task LoadPageDataAsync()
	{
		// Сортировка по DisplayOrder, а при равенстве — по Id,
		// иначе порядок карточек может «прыгать» между запросами.
		Projects = await _context.Projects
			.Where(project => project.IsPublished)
			.OrderBy(project => project.DisplayOrder)
			.ThenBy(project => project.Id)
			.ToListAsync();

		Skills = await _context.Skills
			.OrderBy(skill => skill.DisplayOrder)
			.ThenBy(skill => skill.Name)
			.ToListAsync();
	}
}
