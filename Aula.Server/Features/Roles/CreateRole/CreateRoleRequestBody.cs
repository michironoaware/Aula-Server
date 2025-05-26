using Aula.Server.Domain.AccessControl;

namespace Aula.Server.Features.Roles.CreateRole;

internal sealed record CreateRoleRequestBody
{
	public String? Name { get; init; }

	public Permissions? Permissions { get; init; }
}
