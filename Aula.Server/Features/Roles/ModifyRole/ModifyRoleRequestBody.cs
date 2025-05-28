using Aula.Server.Domain.AccessControl;

namespace Aula.Server.Features.Roles.ModifyRole;

internal sealed record ModifyRoleRequestBody
{
	public String? Name { get; init; }

	public Permissions? Permissions { get; init; }
}
