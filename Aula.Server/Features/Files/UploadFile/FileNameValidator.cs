using Aula.Server.Domain.Files;
using FluentValidation;

namespace Aula.Server.Features.Files.UploadFile;

internal sealed class FileNameValidator : AbstractValidator<String>
{
	private const String PropertyName = "name";

	public FileNameValidator()
	{
		_ = RuleFor(x => x)
			.MinimumLength(File.NameMinLength)
			.WithErrorCode(PropertyName)
			.WithMessage($"Length must be at least {File.NameMinLength}.");

		_ = RuleFor(x => x)
			.MaximumLength(File.NameMaxLength)
			.WithErrorCode(PropertyName)
			.WithMessage($"Length must be at most {File.NameMaxLength}.");
	}
}
