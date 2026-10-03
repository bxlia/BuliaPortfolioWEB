using BuliaPortfolio.Data;
using BuliaPortfolio.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BuliaPortfolio.Pages.Admin;

// Страница просмотра заявок клиентов.
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
		// Новые заявки выводятся первыми; AsNoTracking нужен только для чтения.
		Requests = await _context.ContactRequests
			.AsNoTracking()
			.OrderByDescending(request => request.CreatedAt)
			.ToListAsync();
	}
}