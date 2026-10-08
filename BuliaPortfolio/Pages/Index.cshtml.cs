using BuliaPortfolio.Data;
using BuliaPortfolio.Models;
using BuliaPortfolio.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BuliaPortfolio.Pages;

// Главная страница: показывает проекты и навыки, принимает заявку в SQL Server.
public class IndexModel : PageModel
{
	private readonly PortfolioDbContext _context;
	private readonly MaxBot _maxBot;

	public IndexModel(PortfolioDbContext context, MaxBot maxBot)
	{
		_context = context;
		_maxBot = maxBot;
	}

	public List<Project> Projects { get; private set; } = [];

	public List<Skill> Skills { get; private set; } = [];

	[BindProperty]
	public ContactRequestInput ClientForm { get; set; } = new();

	[BindProperty(SupportsGet = true, Name = "sent")]
	public bool IsRequestSent { get; set; }

	public async Task OnGetAsync()
	{
		// Метка времени нужна форме, чтобы потом понять,
		// сколько секунд её заполняли.
		ClientForm.FormStamp = DateTime.UtcNow.Ticks;

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

		// Форму отправили сразу после загрузки страницы — так работают боты.
		if (SpamGuard.FilledTooFast(ClientForm.FormStamp))
		{
			return RedirectToPage(new { sent = true });
		}

		// С одного адреса слишком много отправок.
		if (SpamGuard.TooManyFrom(SpamGuard.GetClientKey(HttpContext)))
		{
			return RedirectToPage(new { sent = true });
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var request = ClientForm.ToEntity();

		_context.ContactRequests.Add(request);

		await _context.SaveChangesAsync();

		// Уведомление в MAX. Если отправка не вышла, заявка всё равно
		// сохранена — письмо не должно мешать клиенту получить ответ.
		await _maxBot.SendAsync(BuildMessage(request));

		return RedirectToPage(new { sent = true });
	}

	// Текст уведомления для мессенджера. Держим коротким,
	// в MAX не больше 4000 символов, но и засорять чат не стоит.
	private static string BuildMessage(ContactRequest request)
	{
		var text = $"Новая заявка с сайта\n\nИмя: {request.ClientName}\nПочта: {request.Email}";

		if (!string.IsNullOrWhiteSpace(request.Phone))
		{
			text += $"\nТелефон: {request.Phone}";
		}

		text += $"\n\nЗадача: {request.Message}";

		return text.Length > 3500 ? text[..3500] + "…" : text;
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