using BuliaPortfolio.Data;
using BuliaPortfolio.Models;
using BuliaPortfolio.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BuliaPortfolio.Pages.Admin;

[Authorize(Roles = "Admin")]
// Страница просмотра и обработки заявок клиентов.
public class RequestsModel : PageModel
{
	private readonly PortfolioDbContext _context;
	private readonly MaxBot _maxBot;

	public List<ContactRequest> Requests { get; private set; } = new();

	// Результат проверки связи с MAX показываем прямо на странице.
	public string? MaxTestResult { get; private set; }

	public RequestsModel(PortfolioDbContext context, MaxBot maxBot)
	{
		_context = context;
		_maxBot = maxBot;
	}

	public async Task OnGetAsync()
	{
		await LoadRequestsAsync();
	}

	public async Task<IActionResult> OnPostTestMaxAsync()
	{
		MaxTestResult = await _maxBot.TestAsync();

		await LoadRequestsAsync();

		return Page();
	}

	private async Task LoadRequestsAsync()
	{
		Requests = await _context.ContactRequests
			.AsNoTracking()
			.OrderByDescending(request => request.CreatedAt)
			.ToListAsync();
	}

	public async Task<IActionResult> OnPostMarkProcessedAsync(int id)
	{
		// Ищем заявку по её уникальному Id.
		var request = await _context.ContactRequests.FindAsync(id);

		if (request is null)
		{
			return NotFound();
		}

		request.IsProcessed = true;

		await _context.SaveChangesAsync();

		// Перезагрузка страницы показывает обновлённый статус.
		return RedirectToPage();
	}
}