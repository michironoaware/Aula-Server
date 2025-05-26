using Aula.Server.Domain.Users;
using Aula.Server.Shared.Extensions;
using FluentValidation;

namespace Aula.Server.Features.Bots.CreateBot;

internal sealed class CreateBotRequestBodyValidator : AbstractValidator<CreateBotRequestBody>
{
	public CreateBotRequestBodyValidator()
	{
		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(User.DisplayNameMinLength)
			.WithErrorCode(nameof(CreateBotRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at least {User.DisplayNameMinLength}");

		_ = RuleFor(x => x.DisplayName)
			.MaximumLength(User.DisplayNameMaxLength)
			.WithErrorCode(nameof(CreateBotRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at most {User.DisplayNameMaxLength}");
	}
}
