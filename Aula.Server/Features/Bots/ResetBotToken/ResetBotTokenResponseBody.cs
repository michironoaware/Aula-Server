namespace Aula.Server.Features.Bots.ResetBotToken;

internal sealed record ResetBotTokenResponseBody
{
	public required String Token { get; init; }
}
