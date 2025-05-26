using System.Net.Mail;
using Aula.Server.Shared.BackgroundTaskQueue;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Aula.Server.Shared.Mail;

internal static class DependencyInjection
{
	internal static IServiceCollection AddMailSender(this IServiceCollection services)
	{
		_ = services.AddOptions<ApplicationOptions>()
			.BindConfiguration(ApplicationOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.AddOptions<MailOptions>()
			.BindConfiguration(MailOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.TryAddTransient<SmtpClient>();
		services.TryAddSingleton<IEmailSender, DefaultEmailSender>();
		_ = services.AddBackgroundTaskQueueFor<DefaultEmailSender>();
		return services;
	}
}
