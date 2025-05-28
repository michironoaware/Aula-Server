using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Bans;
using Aula.Server.Domain.Files;
using Aula.Server.Domain.Messages;
using Aula.Server.Domain.Rooms;
using Aula.Server.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Shared.Persistence;

internal static class EntityMappers
{
	internal static void MapDomainEntities(this ModelBuilder modelBuilder)
	{
		modelBuilder.MapUsers();
		modelBuilder.MapRoles();
		modelBuilder.MapRooms();
		modelBuilder.MapMessages();
		modelBuilder.MapBans();
		modelBuilder.MapFiles();
	}

	private static void MapUsers(this ModelBuilder builder)
	{
		var userBuilder = builder.Entity<User>();
		_ = userBuilder.Property(e => e.Id);
		_ = userBuilder.Property(e => e.Type);
		_ = userBuilder.HasMany(e => e.RoleAssignments)
			.WithOne(e => e.User)
			.OnDelete(DeleteBehavior.Cascade);
		_ = userBuilder.Property(e => e.DisplayName);
		_ = userBuilder.Property(e => e.Description);
		_ = userBuilder.Property(e => e.Presence);
		_ = userBuilder.HasOne(e => e.CurrentRoom)
			.WithMany(e => e.Residents)
			.OnDelete(DeleteBehavior.Restrict);
		_ = userBuilder.Property(e => e.CreationDate);
		_ = userBuilder.Property(e => e.IsDeleted);
		_ = userBuilder.Property(e => e.SecurityStamp);
		_ = userBuilder.Property(e => e.ConcurrencyStamp);
		_ = userBuilder.HasDiscriminator<UserType>(nameof(User.Type))
			.HasValue<StandardUser>(UserType.Standard)
			.HasValue<BotUser>(UserType.Bot);

		var standardUserBuilder = builder.Entity<StandardUser>();
		_ = standardUserBuilder.Property(e => e.UserName);
		_ = standardUserBuilder.Property(e => e.Email);
		_ = standardUserBuilder.Property(e => e.EmailConfirmed);
		_ = standardUserBuilder.Property(e => e.PasswordHash);
		_ = standardUserBuilder.Property(e => e.AccessFailedCount);
		_ = standardUserBuilder.Property(e => e.LockoutEndTime);
	}

	private static void MapRoles(this ModelBuilder builder)
	{
		var roleBuilder = builder.Entity<Role>();
		_ = roleBuilder.Property(e => e.Id);
		_ = roleBuilder.Property(e => e.Name);
		_ = roleBuilder.Property(e => e.Permissions);
		_ = roleBuilder.Property(e => e.Position);
		_ = roleBuilder.Property(e => e.IsGlobal);
		_ = roleBuilder.HasMany(e => e.RoleAssignments)
			.WithOne(e => e.Role)
			.OnDelete(DeleteBehavior.Cascade);
		_ = roleBuilder.Property(e => e.ConcurrencyStamp);
		_ = roleBuilder.Property(e => e.IsRemoved);

		var roleAssignmentBuilder = builder.Entity<RoleAssignment>();
		_ = roleAssignmentBuilder.Property(e => e.Id);
		_ = roleAssignmentBuilder.HasOne(e => e.Role)
			.WithMany(e => e.RoleAssignments)
			.OnDelete(DeleteBehavior.SetNull);
		_ = roleAssignmentBuilder.HasOne(e => e.User)
			.WithMany(e => e.RoleAssignments)
			.OnDelete(DeleteBehavior.SetNull);
	}

	private static void MapRooms(this ModelBuilder builder)
	{
		var roomBuilder = builder.Entity<Room>();
		_ = roomBuilder.Property(e => e.Id);
		_ = roomBuilder.Property(e => e.Type);
		_ = roomBuilder.Property(e => e.Name);
		_ = roomBuilder.Property(e => e.Description);
		_ = roomBuilder.Property(e => e.IsEntrance);
		_ = roomBuilder.HasOne(e => e.BackgroundAudio)
			.WithMany(e => e.ChatsUsingAsBackgroundAudio)
			.OnDelete(DeleteBehavior.SetNull);
		_ = roomBuilder.HasMany(e => e.Destinations)
			.WithOne(e => e.SourceRoom)
			.OnDelete(DeleteBehavior.Cascade);
		_ = roomBuilder.HasMany(e => e.Origins)
			.WithOne(e => e.TargetRoom)
			.OnDelete(DeleteBehavior.Cascade);
		_ = roomBuilder.Property(e => e.CreationDate);
		_ = roomBuilder.Property(e => e.IsRemoved);
		_ = roomBuilder.Property(e => e.ConcurrencyStamp);
		_ = roomBuilder.HasDiscriminator<RoomType>(nameof(Room.Type))
			.HasValue<StandardRoom>(RoomType.Standard);

		var roomConnectionBuilder = builder.Entity<RoomConnection>();
		_ = roomConnectionBuilder.Property(e => e.Id);
		_ = roomConnectionBuilder.HasOne(e => e.SourceRoom)
			.WithMany(e => e.Destinations)
			.OnDelete(DeleteBehavior.SetNull);
		_ = roomConnectionBuilder.HasOne(e => e.TargetRoom)
			.WithMany(e => e.Origins)
			.OnDelete(DeleteBehavior.SetNull);
	}

	private static void MapMessages(this ModelBuilder builder)
	{
		var messageBuilder = builder.Entity<Message>();
		_ = messageBuilder.Property(e => e.Id);
		_ = messageBuilder.Property(e => e.Type);
		_ = messageBuilder.HasOne(e => e.Room)
			.WithMany(e => e.Messages)
			.OnDelete(DeleteBehavior.SetNull);
		_ = messageBuilder.Property(e => e.AuthorType);
		_ = messageBuilder.HasOne(e => e.Author)
			.WithMany(e => e.MessagesSent)
			.OnDelete(DeleteBehavior.SetNull);
		_ = messageBuilder.Property(e => e.Flags);
		_ = messageBuilder.Property(e => e.CreationDate);
		_ = messageBuilder.Property(e => e.IsRemoved);
		_ = messageBuilder.Property(e => e.ConcurrencyStamp);
		_ = messageBuilder.HasDiscriminator<MessageType>(nameof(Message.Type))
			.HasValue<DefaultMessage>(MessageType.Default)
			.HasValue<UserJoinMessage>(MessageType.UserJoin)
			.HasValue<UserLeaveMessage>(MessageType.UserLeave);

		var textMessageBuilder = builder.Entity<DefaultMessage>();
		_ = textMessageBuilder.Property(e => e.Text);

		var userJoinMessageBuilder = builder.Entity<UserJoinMessage>();
		_ = userJoinMessageBuilder.OwnsOne(e => e.JoinData, e =>
		{
			_ = e.Navigation(e2 => e2.User);
			_ = e.Navigation(e2 => e2.PreviousRoom);
		});

		var userLeaveMessageBuilder = builder.Entity<UserLeaveMessage>();
		_ = userLeaveMessageBuilder.OwnsOne(e => e.LeaveData, e =>
		{
			_ = e.Navigation(e2 => e2.User);
			_ = e.Navigation(e2 => e2.NextRoom);
		});
	}

	private static void MapBans(this ModelBuilder builder)
	{
		var banModel = builder.Entity<Ban>();
		_ = banModel.Property(e => e.Id);
		_ = banModel.Property(e => e.Type);
		_ = banModel.Property(e => e.IssuerType);
		_ = banModel.HasOne(e => e.Issuer)
			.WithMany(e => e.BansIssued)
			.OnDelete(DeleteBehavior.SetNull);
		_ = banModel.Property(e => e.Reason);
		_ = banModel.Property(e => e.EmissionDate);
		_ = banModel.Property(e => e.IsLifted);
		_ = banModel.Property(e => e.ConcurrencyStamp);
		_ = banModel.HasDiscriminator<BanType>(nameof(Ban.Type))
			.HasValue<UserBan>(BanType.User);

		var userBanModel = builder.Entity<UserBan>();
		_ = userBanModel.HasOne(e => e.TargetUser)
			.WithMany(e => e.BansReceived)
			.OnDelete(DeleteBehavior.SetNull);
	}

	private static void MapFiles(this ModelBuilder builder)
	{
		var fileBuilder = builder.Entity<File>();
		_ = fileBuilder.Property(e => e.Id);
		_ = fileBuilder.Property(e => e.Name);
		_ = fileBuilder.Property(e => e.ContentType);
		_ = fileBuilder.Property(e => e.Content);
		_ = fileBuilder.Property(e => e.Size);
		_ = fileBuilder.Property(e => e.UploadDate);
		_ = fileBuilder.HasOne(e => e.Submitter)
			.WithMany(e => e.FilesSubmitted)
			.OnDelete(DeleteBehavior.SetNull);
		_ = fileBuilder.Property(e => e.ConcurrencyStamp);
		_ = fileBuilder.Property(e => e.IsRemoved);
	}
}
