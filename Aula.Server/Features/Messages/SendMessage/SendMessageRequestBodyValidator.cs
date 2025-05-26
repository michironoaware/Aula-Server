using Aula.Server.Domain.Messages;
using Aula.Server.Shared.Extensions;
using FluentValidation;

namespace Aula.Server.Features.Messages.SendMessage;

internal sealed class SendMessageRequestBodyValidator : AbstractValidator<SendMessageRequestBody>
{
	public SendMessageRequestBodyValidator()
	{
		var allowedMessageTypes = new[] { MessageType.Default };
		var messageFlags = Enum.GetValues<MessageFlags>();

		_ = RuleFor(x => x.Type)
			.IsInEnum()
			.Must(v => allowedMessageTypes.Any(allowedType => v == allowedType))
			.WithErrorCode(nameof(SendMessageRequestBody.Type).ToCamel())
			.WithMessage($"Invalid value. Valid values are: {String.Join(", ", allowedMessageTypes.Cast<Int32>())}.");

		_ = RuleFor(x => x.Flags)
			.IsInEnum()
			.WithErrorCode(nameof(SendMessageRequestBody.Flags).ToCamel())
			.WithMessage("Unknown value. " +
				$"Known values are combinations of the following flags: {String.Join(", ", messageFlags.Cast<UInt64>())}.");

		_ = When(x => x.Type is MessageType.Default, () =>
		{
			_ = RuleFor(x => x.Text)
				.NotNull()
				.WithErrorCode(nameof(SendMessageRequestBody.Text).ToCamel())
				.WithMessage(
					$"Required when {nameof(SendMessageRequestBody.Type).ToCamel()} is {(Int32)MessageType.Default}.");

			_ = RuleFor(x => x.Text)
				.MinimumLength(DefaultMessage.TextMinLength)
				.WithErrorCode(nameof(SendMessageRequestBody.Text).ToCamel())
				.WithMessage($"Length must be at least {DefaultMessage.TextMinLength}.");

			_ = RuleFor(x => x.Text)
				.MaximumLength(DefaultMessage.TextMaxLength)
				.WithErrorCode(nameof(SendMessageRequestBody.Text).ToCamel())
				.WithMessage($"Length must be at most {DefaultMessage.TextMinLength}.");
		});
	}
}
