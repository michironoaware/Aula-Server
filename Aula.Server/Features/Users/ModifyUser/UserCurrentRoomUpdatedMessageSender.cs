using Aula.Server.Domain.Messages;
using Aula.Server.Domain.Users;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using MediatR;

namespace Aula.Server.Features.Users.ModifyUser;

internal sealed class UserCurrentRoomUpdatedMessageSender : INotificationHandler<UserCurrentRoomUpdatedEvent>
{
	private readonly AppDbContext _dbContext;
	private readonly ISnowflakeGenerator _snowflakes;

	public UserCurrentRoomUpdatedMessageSender(
		AppDbContext dbContext,
		ISnowflakeGenerator snowflakes)
	{
		_dbContext = dbContext;
		_snowflakes = snowflakes;
	}

	public async Task Handle(UserCurrentRoomUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var messageId = await _snowflakes.GenerateAsync();
		Message message;
		if (notification.PreviousRoomId is not null)
		{
			message = new UserLeaveMessage(messageId, 0, null, (Snowflake)notification.PreviousRoomId,
				new MessageUserLeave(notification.UserId, notification.CurrentRoomId));
		}
		else if (notification.CurrentRoomId is not null)
		{
			message = new UserJoinMessage(messageId, 0, null, (Snowflake)notification.CurrentRoomId,
				new MessageUserJoin(notification.UserId, notification.PreviousRoomId));
		}
		else
			return;

		_ = _dbContext.Messages.Add(message);
		_ = await _dbContext.SaveChangesAsync(cancellationToken);
	}
}
