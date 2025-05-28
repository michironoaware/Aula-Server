namespace Aula.Server.Features.Roles.UpdateRolePositions;

internal sealed class RolePositionByIdEqualityComparer : IEqualityComparer<UpdateRolePositionsRequestBodyRole>
{
	public Boolean Equals(UpdateRolePositionsRequestBodyRole? x, UpdateRolePositionsRequestBodyRole? y) =>
		x?.Id == y?.Id;

	public Int32 GetHashCode(UpdateRolePositionsRequestBodyRole obj) =>
		obj.Id.GetHashCode();

	public static RolePositionByIdEqualityComparer Instance { get; } = new();
}
