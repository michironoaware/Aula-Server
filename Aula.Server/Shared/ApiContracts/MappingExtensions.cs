using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Bans;
using Aula.Server.Domain.Messages;
using Aula.Server.Domain.Rooms;
using Aula.Server.Domain.Users;

namespace Aula.Server.Shared.ApiContracts;

internal static class MappingExtensions
{
	internal static UserData ToUserData(this User user) => new()
	{
		Id = user.Id,
		DisplayName = user.DisplayName,
		Description = user.Description,
		CurrentRoomId = user.CurrentRoomId,
		Presence = user.Presence,
		Type = user.Type,
		RoleIds = user.RoleAssignments.Select(ra => ra.RoleId),
	};

	internal static RoomData ToRoomData(this Room room) => new()
	{
		Id = room.Id,
		Name = room.Name,
		Description = room.Description,
		Type = room.Type,
		BackgroundAudioId = room.BackgroundAudioId,
		IsEntrance = room.IsEntrance,
		DestinationIds = room.Destinations.Select(rc => rc.DestinationRoomId),
		ResidentIds = room.Residents.Select(u => u.Id),
		CreationDate = room.CreationDate,
	};

	internal static MessageData ToMessageData(this Message message) => new()
	{
		Id = message.Id,
		Type = message.Type,
		Flags = message.Flags,
		AuthorType = message.AuthorType,
		AuthorId = message.AuthorId,
		RoomId = message.RoomId,
		CreationDate = message.CreationDate,
		Text = message is DefaultMessage textMessage ? textMessage.Text : null,
		JoinData = message is UserJoinMessage joinMessage
			? new MessageUserJoinData
			{
				UserId = joinMessage.JoinData.UserId, PreviousRoomId = joinMessage.JoinData.PreviousRoomId,
			}
			: null,
		LeaveData = message is UserLeaveMessage leaveMessage
			? new MessageUserLeaveData
			{
				UserId = leaveMessage.LeaveData.UserId, NextRoomId = leaveMessage.LeaveData.NextRoomId,
			}
			: null,
	};

	internal static BanData ToBanData(this Ban ban) => new()
	{
		Id = ban.Id,
		Type = ban.Type,
		IssuerType = ban.IssuerType,
		IssuerId = ban.IssuerId,
		Reason = ban.Reason,
		TargetId = ban is UserBan userBan ? userBan.TargetUserId : null,
		IsLifted = ban.IsLifted,
		EmissionDate = ban.EmissionDate,
	};

	internal static RoleData ToRoleData(this Role role) => new()
	{
		Id = role.Id, Name = role.Name, Permissions = role.Permissions, Index = role.Position,
	};
}
