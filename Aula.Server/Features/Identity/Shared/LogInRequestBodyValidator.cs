using Aula.Server.Shared.Extensions;
using FluentValidation;

namespace Aula.Server.Features.Identity.Shared;

internal sealed class LogInRequestBodyValidator : AbstractValidator<LogInRequestBody>
{
	public LogInRequestBodyValidator()
	{
		_ = RuleFor(x => x.UserName)
			.NotEmpty()
			.WithErrorCode(nameof(LogInRequestBody.UserName).ToCamel())
			.WithMessage("Cannot be null or empty.");

		_ = RuleFor(x => x.Password)
			.NotEmpty()
			.WithErrorCode(nameof(LogInRequestBody.UserName).ToCamel())
			.WithMessage("Cannot be null or empty.");
	}
}
