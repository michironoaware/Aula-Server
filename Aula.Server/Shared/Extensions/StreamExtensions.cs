using System.IO;

namespace Aula.Server.Shared.Extensions;

internal static class StreamExtensions
{
	internal static async ValueTask<Byte[]> ToArrayAsync(
		this Stream stream,
		CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();

		if (!stream.CanRead)
			throw new ArgumentException("Stream must be readable.", nameof(stream));

		if (!stream.CanSeek)
			throw new ArgumentException("Stream must be seekable.", nameof(stream));

		var initialPosition = stream.Position;
		var bytes = new Byte[stream.Length];

		_ = stream.Seek(0, SeekOrigin.Begin);
		_ = await stream.ReadAtLeastAsync(bytes, bytes.Length, true, ct);
		_ = stream.Seek(initialPosition, SeekOrigin.Begin);

		return bytes;
	}
}
