using BuliaPortfolio.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Подключаем EF Core к SQL Server через строку DefaultConnection.
builder.Services.AddDbContext<PortfolioDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DefaultConnection")));

// Подключаем Identity, роли и настройки пароля администратора.
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
.AddEntityFrameworkStores<PortfolioDbContext>();

builder.Services.AddRazorPages();

var app = builder.Build();

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

// Создаём роль и локальную учётную запись администратора при разработке.
if (app.Environment.IsDevelopment())
{
	using var scope = app.Services.CreateScope();

	var roleManager = scope.ServiceProvider
		.GetRequiredService<RoleManager<IdentityRole>>();

	var userManager = scope.ServiceProvider
		.GetRequiredService<UserManager<IdentityUser>>();

	const string adminRole = "Admin";

	if (!await roleManager.RoleExistsAsync(adminRole))
	{
		await roleManager.CreateAsync(new IdentityRole(adminRole));
	}

	var adminEmail = builder.Configuration["Admin:Email"];
	var adminPassword = builder.Configuration["Admin:Password"];

	if (!string.IsNullOrWhiteSpace(adminEmail) &&
		!string.IsNullOrWhiteSpace(adminPassword))
	{
		var admin = await userManager.FindByEmailAsync(adminEmail);

		if (admin is null)
		{
			admin = new IdentityUser
			{
				UserName = adminEmail,
				Email = adminEmail,
				EmailConfirmed = true
			};

			var createResult = await userManager.CreateAsync(admin, adminPassword);

			if (!createResult.Succeeded)
			{
				var errors = string.Join(
					"; ",
					createResult.Errors.Select(error => error.Description));

				throw new InvalidOperationException(
					$"Не удалось создать администратора: {errors}");
			}
		}

		if (!await userManager.IsInRoleAsync(admin, adminRole))
		{
			await userManager.AddToRoleAsync(admin, adminRole);
		}
	}
}

app.Run();