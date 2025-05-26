using Aula.Server.Shared.Extensions;
using FluentValidation;

namespace Aula.Server.Features.Roles.UpdateRolePositions;

internal sealed class UpdateRolePositionsRequestBodyRoleValidator
	: AbstractValidator<UpdateRolePositionsRequestBodyRole>
{
	public UpdateRolePositionsRequestBodyRoleValidator()
	{
		_ = RuleFor(x => x.Position)
			.LessThanOrEqualTo(0)
			.WithErrorCode(nameof(UpdateRolePositionsRequestBodyRole.Position).ToCamel())
			.WithMessage("Cannot be less than or equal to 0");
	}
}
