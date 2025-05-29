using Aula.Server.Domain.Files;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Gateway;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.FileCreated;

internal sealed class FileCreatedEventDispatcher : INotificationHandler<FileCreatedEvent>
{
	private readonly GatewayManager _gatewayManager;

	public FileCreatedEventDispatcher(GatewayManager gatewayManager)
	{
		_gatewayManager = gatewayManager;
	}

	public async Task Handle(FileCreatedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<FileData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.FileCreated,
			Data = notification.File.ToFileData(),
		};

		await _gatewayManager.DispatchEventAsync(payload);
	}
}
