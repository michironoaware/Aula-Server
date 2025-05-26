using Aula.Server.Domain.Users;
using Aula.Server.Shared.Extensions;
using Aula.Server.Shared.Identity;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Aula.Server.Features.Identity.ResetPassword;

internal sealed class ResetPasswordRequestBodyValidator : AbstractValidator<ResetPasswordRequestBody>
{
	public ResetPasswordRequestBodyValidator(IOptions<IdentityOptions> options)
	{
		_ = RuleFor(x => x.Code)
			.NotEmpty()
			.WithErrorCode(nameof(ResetPasswordRequestBody.Code))
			.WithMessage("Cannot be null or empty.");

		_ = RuleFor(x => x.NewPassword)
			.MinimumLength(options.Value.Password.RequiredLength)
			.WithErrorCode(nameof(ResetPasswordRequestBody.NewPassword).ToCamel())
			.WithMessage($"Length must be at least {options.Value.Password.RequiredLength}.");

		_ = RuleFor(x => x.NewPassword)
			.MaximumLength(StandardUser.PasswordMaxLength)
			.WithErrorCode(nameof(ResetPasswordRequestBody.NewPassword).ToCamel())
			.WithMessage($"Length must be at most {StandardUser.PasswordMaxLength}.");
	}
}
