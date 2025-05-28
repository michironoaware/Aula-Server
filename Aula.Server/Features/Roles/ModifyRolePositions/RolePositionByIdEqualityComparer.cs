namespace Aula.Server.Features.Roles.ModifyRolePositions;

internal sealed class RolePositionByIdEqualityComparer : IEqualityComparer<ModifyRolePositionsRequestBodyRole>
{
	public Boolean Equals(ModifyRolePositionsRequestBodyRole? x, ModifyRolePositionsRequestBodyRole? y) =>
		x?.Id == y?.Id;

	public Int32 GetHashCode(ModifyRolePositionsRequestBodyRole obj) =>
		obj.Id.GetHashCode();

	public static RolePositionByIdEqualityComparer Instance { get; } = new();
}
