using Aula.Server.Domain.Bans;
using Aula.Server.Shared.Extensions;
using FluentValidation;

namespace Aula.Server.Features.Users.BanUser;

internal sealed class BanUserRequestBodyValidator : AbstractValidator<BanUserRequestBody>
{
	public BanUserRequestBodyValidator()
	{
		_ = RuleFor(x => x.Reason)
			.MinimumLength(Ban.ReasonMinLength)
			.WithErrorCode(nameof(BanUserRequestBody.Reason).ToCamel())
			.WithMessage($"Length must be at least {Ban.ReasonMinLength}");

		_ = RuleFor(x => x.Reason)
			.MaximumLength(Ban.ReasonMaxLength)
			.WithErrorCode(nameof(BanUserRequestBody.Reason).ToCamel())
			.WithMessage($"Length must be at most {Ban.ReasonMaxLength}");
	}
}
