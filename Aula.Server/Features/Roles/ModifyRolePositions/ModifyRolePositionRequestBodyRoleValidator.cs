using Aula.Server.Shared.Extensions;
using FluentValidation;

namespace Aula.Server.Features.Roles.ModifyRolePositions;

internal sealed class UpdateRolePositionsRequestBodyRoleValidator
	: AbstractValidator<ModifyRolePositionsRequestBodyRole>
{
	public UpdateRolePositionsRequestBodyRoleValidator()
	{
		_ = RuleFor(x => x.Position)
			.LessThanOrEqualTo(0)
			.WithErrorCode(nameof(ModifyRolePositionsRequestBodyRole.Position).ToCamel())
			.WithMessage("Cannot be less than or equal to 0");
	}
}
