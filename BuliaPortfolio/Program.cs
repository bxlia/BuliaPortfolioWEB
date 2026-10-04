using BuliaPortfolio.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Подключение к SQL Server. Строка берётся из user secrets (локально)
// или из переменной окружения ConnectionStrings__DefaultConnection (сервер).
builder.Services.AddDbContext<PortfolioDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DefaultConnection")));

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

var app = builder.Build();

// Страница ошибки и 404 работают одинаково и при разработке, и на сервере.
app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();

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

app.Run();
