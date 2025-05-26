using System.Threading.Channels;

namespace Aula.Server.Shared.BackgroundTaskQueue;

/// <summary>
///     An implementation of <see cref="IBackgroundTaskQueue{T}" /> that uses <see cref="Channel" />.
/// </summary>
internal sealed class DefaultBackgroundTaskQueue<T> : IBackgroundTaskQueue<T>
{
	private readonly Channel<Func<CancellationToken, ValueTask>> _taskQueue =
		Channel.CreateUnbounded<Func<CancellationToken, ValueTask>>();

	public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem)
	{
		ArgumentNullException.ThrowIfNull(workItem, nameof(workItem));
		_ = await _taskQueue.Writer.WaitToWriteAsync();
		await _taskQueue.Writer.WriteAsync(workItem);
	}

	public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken ct)
	{
		_ = await _taskQueue.Reader.WaitToReadAsync(ct);
		return await _taskQueue.Reader.ReadAsync(ct);
	}
}
