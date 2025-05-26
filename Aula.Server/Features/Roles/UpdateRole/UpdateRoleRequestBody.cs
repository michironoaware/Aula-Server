using Aula.Server.Domain.AccessControl;

namespace Aula.Server.Features.Roles.UpdateRole;

internal sealed record UpdateRoleRequestBody
{
	public String? Name { get; init; }

	public Permissions? Permissions { get; init; }
}
