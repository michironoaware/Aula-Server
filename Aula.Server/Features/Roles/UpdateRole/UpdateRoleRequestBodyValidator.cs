using Aula.Server.Domain.AccessControl;
using Aula.Server.Shared.Extensions;
using FluentValidation;

namespace Aula.Server.Features.Roles.UpdateRole;

internal sealed class UpdateRoleRequestBodyValidator : AbstractValidator<UpdateRoleRequestBody>
{
	public UpdateRoleRequestBodyValidator()
	{
		_ = RuleFor(x => x.Name)
			.MinimumLength(Role.NameMinLength)
			.WithErrorCode(nameof(UpdateRoleRequestBody.Name).ToCamel())
			.WithMessage($"Length must be at least {Role.NameMinLength}");

		_ = RuleFor(x => x.Name)
			.MaximumLength(Role.NameMaxLength)
			.WithErrorCode(nameof(UpdateRoleRequestBody.Name).ToCamel())
			.WithMessage($"Length must be at most {Role.NameMaxLength}");

		_ = RuleFor(x => x)
			.Must(x =>
				x.Name is not null ||
				x.Permissions is not null)
			.WithErrorCode(nameof(UpdateRoleRequestBody.Name).ToCamel())
			.WithMessage("At least one property must be provided.");
	}
}
