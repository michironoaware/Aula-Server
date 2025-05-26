using Aula.Server.Domain.AccessControl;
using Aula.Server.Shared.Extensions;
using FluentValidation;

namespace Aula.Server.Features.Roles.CreateRole;

internal sealed class CreateRoleRequestBodyValidator : AbstractValidator<CreateRoleRequestBody>
{
	public CreateRoleRequestBodyValidator()
	{
		_ = RuleFor(x => x.Name)
			.MinimumLength(Role.NameMinLength)
			.WithErrorCode(nameof(CreateRoleRequestBody.Name).ToCamel())
			.WithMessage($"Length must be at least {Role.NameMinLength}");

		_ = RuleFor(x => x.Name)
			.MaximumLength(Role.NameMaxLength)
			.WithErrorCode(nameof(CreateRoleRequestBody.Name).ToCamel())
			.WithMessage($"Length must be at most {Role.NameMaxLength}");
	}
}
