using System.ComponentModel.DataAnnotations;

namespace BuliaPortfolio.Models;

// Данные формы заявки. Отдельный класс от сущности ContactRequest,
// чтобы правила проверки формы не применялись к таблице.
// Ограничения по длине совпадают с размерами колонок в SQL Server.
public class ContactRequestInput
{
	[Required(ErrorMessage = "Укажите, пожалуйста, ваше имя.")]
	[StringLength(300, ErrorMessage = "Имя не должно быть длиннее 300 символов.")]
	public string ClientName { get; set; } = string.Empty;

	[Required(ErrorMessage = "Укажите, пожалуйста, email.")]
	[EmailAddress(ErrorMessage = "Это не похоже на email.")]
	[StringLength(510, ErrorMessage = "Email не должен быть длиннее 510 символов.")]
	public string ClientEmail { get; set; } = string.Empty;

	[Phone(ErrorMessage = "Это не похоже на номер телефона.")]
	[StringLength(100, ErrorMessage = "Телефон не должен быть длиннее 100 символов.")]
	public string? ClientPhone { get; set; }

	[Required(ErrorMessage = "Опишите, пожалуйста, задачу.")]
	[StringLength(4000, ErrorMessage = "Описание не должно быть длиннее 4000 символов.")]
	public string ClientMessage { get; set; } = string.Empty;

	// Метка времени, когда страницу с формой открыли.
	// Если форму отправили быстрее трёх секунд — считаем бота.
	public long FormStamp { get; set; }

	// Honeypot: скрытое поле для ботов. Должно остаться пустым.
	// Если его заполнили — заявка silently игнорируется.
	public string? Website { get; set; }

	public ContactRequest ToEntity() => new ContactRequest
	{
		ClientName = ClientName.Trim(),
		Email = ClientEmail.Trim(),
		Phone = string.IsNullOrWhiteSpace(ClientPhone) ? null : ClientPhone.Trim(),
		Message = ClientMessage.Trim(),
		IsProcessed = false,
		CreatedAt = DateTime.UtcNow
	};
}
