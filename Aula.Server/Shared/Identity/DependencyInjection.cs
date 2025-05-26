using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Aula.Server.Shared.Identity;

internal static class DependencyInjection
{
	internal static IServiceCollection AddIdentity(this IServiceCollection services)
	{
		_ = services.AddOptions<IdentityOptions>()
			.BindConfiguration(IdentityOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.TryAddScoped<UserManager>();
		services.TryAddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
		_ = services.AddHostedService<EmailConfirmationCleanupService>();
		_ = services.AddHostedService<PasswordResetCleanupService>();

		_ = services.Configure<PasswordHasherOptions>(static options =>
		{
			options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
			options.IterationCount = 200000;
		});

		return services;
	}
}
