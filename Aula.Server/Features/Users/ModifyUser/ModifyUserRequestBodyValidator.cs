using Aula.Server.Domain.Users;
using Aula.Server.Shared.Extensions;
using FluentValidation;

namespace Aula.Server.Features.Users.ModifyUser;

internal sealed class ModifyUserRequestBodyValidator : AbstractValidator<ModifyUserRequestBody>
{
	public ModifyUserRequestBodyValidator()
	{
		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(User.DisplayNameMinLength)
			.WithErrorCode(nameof(ModifyUserRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at least {User.DisplayNameMinLength}");

		_ = RuleFor(x => x.DisplayName)
			.MaximumLength(User.DisplayNameMaxLength)
			.WithErrorCode(nameof(ModifyUserRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at most {User.DisplayNameMaxLength}");

		_ = RuleFor(x => x.Description)
			.MaximumLength(User.DescriptionMaxLength)
			.WithErrorCode(nameof(ModifyUserRequestBody.Description).ToCamel())
			.WithMessage($"Length must be at most {User.DescriptionMaxLength}");

		_ = RuleFor(x => x)
			.Must(x =>
				x.DisplayName is not null ||
				x.Description is not null ||
				x.CurrentRoomId is not null ||
				x.RoleIds is not null)
			.WithErrorCode(nameof(ModifyUserRequestBody.DisplayName).ToCamel())
			.WithMessage("At least one property must be provided.");
	}
}
