namespace Aula.Server.Features.Bots.ResetBot;

internal sealed record ResetBotTokenResponseBody
{
	public required String Token { get; init; }
}
