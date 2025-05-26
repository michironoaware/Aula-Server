using System.Net;
using System.Net.Mail;
using Aula.Server.Shared.BackgroundTaskQueue;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace Aula.Server.Shared.Mail;

internal sealed class DefaultEmailSender : IEmailSender
{
	private readonly MailAddress _mailAddress;
	private readonly SmtpClient _smtpClient;
	private readonly IBackgroundTaskQueue<DefaultEmailSender> _taskQueue;

	public DefaultEmailSender(
		SmtpClient smtpClient,
		IOptions<ApplicationOptions> applicationOptions,
		IOptions<MailOptions> mailOptions,
		IBackgroundTaskQueue<DefaultEmailSender> taskQueue)
	{
		_taskQueue = taskQueue;
		_mailAddress = new MailAddress(mailOptions.Value.Address, applicationOptions.Value.Name);

		smtpClient.Host = mailOptions.Value.SmtpHost;
		smtpClient.Port = mailOptions.Value.SmtpPort.Value;
		smtpClient.Credentials = new NetworkCredential(mailOptions.Value.Address, mailOptions.Value.Password);
		smtpClient.EnableSsl = mailOptions.Value.EnableSsl.Value;
		_smtpClient = smtpClient;
	}

	public async Task SendEmailAsync(String email, String subject, String htmlMessage)
	{
		await _taskQueue.QueueBackgroundWorkItemAsync(async ct =>
		{
			using var message = new MailMessage();
			message.From = _mailAddress;
			message.To.Add(email);
			message.Subject = subject;
			message.Body = htmlMessage;
			message.IsBodyHtml = true;

			await _smtpClient.SendMailAsync(message, ct);
		});
	}
}
