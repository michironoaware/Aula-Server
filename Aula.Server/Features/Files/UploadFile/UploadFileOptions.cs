namespace Aula.Server.Features.Files.UploadFile;

internal sealed class UploadFileOptions
{
	internal const String SectionName = "Files";

	internal Int64 AudioMaxByteSize { get; set; } = 1024 * 1024 * 20;

	internal Int64 ImageMaxByteSize { get; set; } = 1024 * 1024 * 20;
}
