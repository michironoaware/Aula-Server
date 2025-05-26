namespace Aula.Server.Shared.Authorization;

internal static class DependencyInjection
{
	/// <summary>
	///     Adds the authorization services used by the application.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
	/// <returns>A reference to this instance after the operation has completed.</returns>
	internal static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
	{
		_ = services.AddAuthorizationBuilder()
			.AddAuthenticatedUserPolicy()
			.AddBanPolicy()
			.AddUserTypePolicy()
			.AddConfirmedEmailPolicy()
			.AddPermissionsPolicy();

		return services;
	}
}
