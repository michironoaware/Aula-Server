namespace Aula.Server.Shared.BackgroundTaskQueue;

/// <summary>
///     A service that queues work to be sequentially executed later.
/// </summary>
/// <typeparam name="T">A marker type parameter used to identity different instances in a dependency injection environment.</typeparam>
internal interface IBackgroundTaskQueue<T>
{
	/// <summary>
	///     Adds work to the end of queue.
	/// </summary>
	/// <param name="workItem">The work to delegate.</param>
	/// <returns>A task representing the queueing operation.</returns>
	ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);

	/// <summary>
	///     Retrieves and removes the next work item from the queue.
	/// </summary>
	/// <param name="ct">A cancellation token to cancel the dequeue operation.</param>
	/// <returns>
	///     A task representing the dequeue operation, containing the next work item.
	/// </returns>
	ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken ct);
}
