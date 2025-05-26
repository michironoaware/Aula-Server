using Aula.Server.Shared.ApiContracts;

namespace Aula.Server.Features.Bots.CreateBot;

internal sealed record CreateBotResponseBody
{
	public required UserData User { get; init; }

	public required String Token { get; init; }
}
