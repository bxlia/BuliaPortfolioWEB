using BuliaPortfolio.Data;
using BuliaPortfolio.Models;
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

	public List<ContactRequest> Requests { get; private set; } = new();

	public RequestsModel(PortfolioDbContext context)
	{
		_context = context;
	}

	public async Task OnGetAsync()
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