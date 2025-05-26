using Aula.Server.Domain.Users;
using Aula.Server.Shared.Extensions;
using Aula.Server.Shared.Identity;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Aula.Server.Features.Identity.Register;

internal sealed class RegisterRequestBodyValidator : AbstractValidator<RegisterRequestBody>
{
	public RegisterRequestBodyValidator(IOptions<IdentityOptions> options)
	{
		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(User.DisplayNameMinLength)
			.WithErrorCode(nameof(RegisterRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at least {User.DisplayNameMinLength}.");

		_ = RuleFor(x => x.DisplayName)
			.MaximumLength(User.DisplayNameMaxLength)
			.WithErrorCode(nameof(RegisterRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at most {User.DisplayNameMaxLength}.");

		_ = RuleFor(x => x.UserName)
			.MinimumLength(StandardUser.UserNameMinLength)
			.WithErrorCode(nameof(RegisterRequestBody.UserName).ToCamel())
			.WithMessage($"Length must be at least {StandardUser.UserNameMinLength}.");

		_ = RuleFor(x => x.UserName)
			.MaximumLength(StandardUser.UserNameMaxLength)
			.WithErrorCode(nameof(RegisterRequestBody.UserName).ToCamel())
			.WithMessage($"Length must be at most {StandardUser.UserNameMaxLength}.");

		_ = RuleFor(x => x.Email)
			.EmailAddress()
			.WithErrorCode(nameof(RegisterRequestBody.Email).ToCamel())
			.WithMessage("Must be a valid email address.");

		_ = RuleFor(x => x.Password)
			.MinimumLength(options.Value.Password.RequiredLength)
			.WithErrorCode(nameof(RegisterRequestBody.Password).ToCamel())
			.WithMessage($"Length must be at least {options.Value.Password.RequiredLength}.");

		_ = RuleFor(x => x.Password)
			.MaximumLength(StandardUser.PasswordMaxLength)
			.WithErrorCode(nameof(RegisterRequestBody.Password).ToCamel())
			.WithMessage($"Length must be at most {StandardUser.PasswordMaxLength}.");
	}
}
