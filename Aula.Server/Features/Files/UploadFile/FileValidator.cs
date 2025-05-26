using Aula.Server.Domain.Files;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Aula.Server.Features.Files.UploadFile;

internal sealed class FileValidator : AbstractValidator<IFormFile>
{
	private const String PropertyName = "file";
	private static readonly String[] s_allowedContentTypes = [ "audio/ogg" ];

	public FileValidator(IOptions<UploadFileOptions> options)
	{
		_ = RuleFor(x => x.ContentType)
			.Must(s_allowedContentTypes.Contains)
			.WithErrorCode(PropertyName)
			.WithMessage($"File content type must be any of: {String.Join(", ", s_allowedContentTypes)}.");

		_ = RuleFor(x => x.ContentType)
			.MaximumLength(File.ContentTypeMaxLength)
			.WithErrorCode(PropertyName)
			.WithMessage($"File Content-Type must be at most {File.ContentTypeMaxLength} characters.");

		_ = When(x => x.ContentType.StartsWith("audio"), () =>
		{
			var maxByteSize = options.Value.AudioMaxByteSize;
			_ = RuleFor(x => x.Length)
				.LessThanOrEqualTo(maxByteSize)
				.WithErrorCode(PropertyName)
				.WithMessage($"File size must be at most {maxByteSize} bytes.");
		});

		_ = When(x => x.ContentType.StartsWith("image"), () =>
		{
			var maxByteSize = options.Value.ImageMaxByteSize;
			_ = RuleFor(x => x.Length)
				.LessThanOrEqualTo(maxByteSize)
				.WithErrorCode(PropertyName)
				.WithMessage($"File size must be at most {maxByteSize} bytes.");
		});
	}
}
