using BuliaPortfolio.Data;
using BuliaPortfolio.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Локальная строка подключения лежит в appsettings.Development.json.
// Этот файл читается только в режиме Development и на сервер не публикуется.
// На хостинге строка подключения приходит из переменной
// ConnectionStrings__DefaultConnection.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
	throw new InvalidOperationException(
		"Не найдена строка подключения. Локально проверь appsettings.Development.json, на сервере — переменную ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<PortfolioDbContext>(options =>
	options.UseSqlServer(connectionString));

// Identity: пользователи, роли и встроенные страницы входа/регистрации.
// AddDefaultUI() включает страницы /Identity/Account/Login и т.д.
// Без него ссылка «Войти» в _LoginPartial ведёт на 404.
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
	options.SignIn.RequireConfirmedAccount = false;

	options.Password.RequiredLength = 10;
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireNonAlphanumeric = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<PortfolioDbContext>()
.AddDefaultUI();

builder.Services.AddRazorPages();

// Ключи шифрования (куки входа и защита форм) храним в файлах.
// Без этого после каждого перезапуска сайта они создаются заново:
// старые формы перестают отправляться, а вход в админку слетает.
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys");
Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
	.PersistKeysToFileSystem(new DirectoryInfo(keysPath))
	.SetApplicationName("BuliaPortfolio");

// Уведомления о заявках в мессенджер MAX.
builder.Services.AddHttpClient("max", client =>
{
	client.Timeout = TimeSpan.FromSeconds(20);
});

builder.Services.AddScoped<MaxBot>();

var app = builder.Build();

// Страница ошибки и 404 работают одинаково и при разработке, и на сервере.
app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();

// Регистрация закрыта: посторонним заводить аккаунты не нужно.
// Вход и админка с заявками остаются доступными по прямой ссылке.
app.UseWhen(
	context => context.Request.Path.Equals("/Identity/Account/Register", StringComparison.OrdinalIgnoreCase),
	branch => branch.Run(context =>
	{
		context.Response.StatusCode = StatusCodes.Status404NotFound;
		return Task.CompletedTask;
	}));

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Роль Admin создаётся всегда, а аккаунт администратора — только если
// заданы Admin:Email и Admin:Password. Это единственный способ завести
// админа на сервере, где нет режима Development.
await SeedData.EnsureAdminAsync(app.Services);

// Если токен MAX задан, а ID чата ещё нет — ищем его по непрочитанным
// событиям. Нужно один раз написать боту любое сообщение в MAX.
await MaxBot.DiscoverChatIdAsync(app.Services);

app.Run();
