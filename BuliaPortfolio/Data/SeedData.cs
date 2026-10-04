using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BuliaPortfolio.Data;

// Создание роли и учётной записи администратора при старте приложения.
// Нужно, чтобы /Admin/Requests был доступен и на сервере, где нет режима
// Development. Пароль и почта берутся из конфигурации, а не из кода.
public static class SeedData
{
	public const string AdminRole = "Admin";

	public static async Task EnsureAdminAsync(IServiceProvider services)
	{
		using var scope = services.CreateScope();

		var provider = scope.ServiceProvider;

		var configuration = provider
			.GetRequiredService<IConfiguration>();

		var roleManager = provider
			.GetRequiredService<RoleManager<IdentityRole>>();

		var userManager = provider
			.GetRequiredService<UserManager<IdentityUser>>();

		if (!await roleManager.RoleExistsAsync(AdminRole))
		{
			var roleResult = await roleManager.CreateAsync(new IdentityRole(AdminRole));

			if (!roleResult.Succeeded)
			{
				throw new InvalidOperationException(
					$"Не удалось создать роль {AdminRole}: {Describe(roleResult)}");
			}
		}

		var email = configuration["Admin:Email"];
		var password = configuration["Admin:Password"];

		if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
		{
			return;
		}

		var admin = await userManager.FindByEmailAsync(email);

		if (admin is null)
		{
			admin = new IdentityUser
			{
				UserName = email,
				Email = email,
				EmailConfirmed = true
			};

			var createResult = await userManager.CreateAsync(admin, password);

			if (!createResult.Succeeded)
			{
				throw new InvalidOperationException(
					$"Не удалось создать администратора: {Describe(createResult)}");
			}
		}

		if (!await userManager.IsInRoleAsync(admin, AdminRole))
		{
			var roleResult = await userManager.AddToRoleAsync(admin, AdminRole);

			if (!roleResult.Succeeded)
			{
				throw new InvalidOperationException(
					$"Не удалось выдать роль {AdminRole}: {Describe(roleResult)}");
			}
		}
	}

	private static string Describe(IdentityResult result)
		=> string.Join("; ", result.Errors.Select(error => error.Description));
}
