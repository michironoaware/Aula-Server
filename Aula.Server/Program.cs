#pragma warning disable CA1848

using System.Diagnostics;
using Aula.Server.Features.Files.GetFileContent;
using Aula.Server.Features.Files.UploadFile;
using Aula.Server.Features.Gateway.ConnectToGateway;
using Aula.Server.Features.Identity.ConfirmEmail;
using Aula.Server.Features.Identity.ResetPassword;
using Aula.Server.Features.Identity.Shared;
using Aula.Server.Features.Messages.DeleteMessage;
using Aula.Server.Features.Messages.SendMessage;
using Aula.Server.Features.Presences;
using Aula.Server.Shared.Authentication;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Cli;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Gateway;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Json;
using Aula.Server.Shared.Logging;
using Aula.Server.Shared.Mail;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.RateLimiting;
using Aula.Server.Shared.Resilience;
using Aula.Server.Shared.Snowflakes;
using Aula.Server.Shared.Tokens;
using FluentValidation;
using MartinCostello.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var startTimestamp = Stopwatch.GetTimestamp();
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("configuration.json", true, true);
builder.Configuration.AddJsonFile("../configuration.json", true, true);

builder.Services
	.AddCors(options =>
	{
		options.AddDefaultPolicy(policy =>
		{
			_ = policy.AllowAnyOrigin()
				.AllowAnyHeader()
				.AllowAnyMethod();
		});
	})
	.AddOpenApi()
	.AddOpenApiExtensions(static options => options.XmlDocumentationAssemblies.Add(typeof(Program).Assembly))
	.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton, includeInternalTypes: true)
	.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>())
	.AddMemoryCache();

builder.Services
	.AddCommands<Program>()
	.AddCustomAuthentication()
	.AddCustomAuthorization()
	.AddEndpoints<Program>()
	.AddGateway()
	.AddIdentity()
	.AddJsonFromAssembly<Program>()
	.AddMailSender()
	.AddPersistence(builder.Configuration)
	.AddCustomRateLimiter()
	.AddResilience()
	.AddSnowflakes()
	.AddSingleton<ITokenProvider, Base64UrlTokenProvider>()
	.AddFileContentRequiredServices()
	.AddFileFeaturesSharedServices()
	.AddConnectToGatewayRequiredServices()
	.AddConfirmEmailRequiredServices()
	.AddResetPasswordRequiredServices()
	.AddIdentitySharedServices()
	.AddRemoveMessageRequiredServices()
	.AddSendMessageRequiredServices()
	.AddHostedService<ResetPresencesOnShutdownService>();

builder.Logging
	.ClearProviders()
	.AddLogging();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
	_ = app.UseHttpsRedirection();

app.UseCors()
	.UseWebSockets()
	.UseWebSocketHeaderParsing()
	.UseAuthentication()
	.UseCustomRateLimiting()
	.UseAuthorization()
	.HandleDbUpdateConcurrencyExceptions();

app.MapCommands()
	.MapEndpoints();

if (app.Environment.IsDevelopment())
{
	var docsRoute = app.MapGroup("docs");
	_ = docsRoute.MapOpenApi();
	_ = docsRoute.MapScalarApiReference("/scalar/", options =>
	{
		options.WithTitle($"{nameof(Aula)} API {{documentName}}")
			.WithTheme(ScalarTheme.Alternate)
			.WithDarkMode(false) // hehehe
			.WithSidebar(true)
			.WithDefaultOpenAllTags(false)
			.WithModels(true)
			.WithDefaultHttpClient(ScalarTarget.Node, ScalarClient.Fetch)
			.OpenApiRoutePattern = "/docs/openapi/{documentName}.json";
	});
}

try
{
	await app.StartAsync();
}
catch (AggregateException e)
{
	foreach (var innerException in e.InnerExceptions)
	{
		if (innerException is not OptionsValidationException validationException)
			continue;

		foreach (var failure in validationException.Failures)
			Console.WriteLine(failure);
	}

	if (e.InnerExceptions.Any(innerE => innerE is not OptionsValidationException))
		throw;
	return;
}

var elapsedTime = Stopwatch.GetElapsedTime(startTimestamp);
var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Now listening on: {Urls}", String.Join(" - ", app.Urls));
logger.LogInformation("{ProgramName} is Ready — It only took {Milliseconds} milliseconds!",
	nameof(Aula), (Int32)elapsedTime.TotalMilliseconds);
logger.LogInformation("Type 'help' to see a list of available commands.");
logger.LogInformation("You can press Ctrl+C to shut down.");

await app.WaitForShutdownAsync();
