using Aula.Server.Domain.Rooms;
using Aula.Server.Features.Rooms.CreateRoom;
using Aula.Server.Shared.Extensions;
using FluentValidation;

namespace Aula.Server.Features.Rooms.ModifyRoom;

internal sealed class ModifyRoomRequestBodyValidator : AbstractValidator<ModifyRoomRequestBody>
{
	public ModifyRoomRequestBodyValidator()
	{
		_ = RuleFor(x => x.Name)
			.MinimumLength(Room.NameMinLength)
			.WithErrorCode(nameof(CreateRoomRequestBody.Name).ToCamel())
			.WithMessage($"Length must be at least {Room.NameMinLength}");

		_ = RuleFor(x => x.Name)
			.MaximumLength(Room.NameMaxLength)
			.WithErrorCode(nameof(CreateRoomRequestBody.Name).ToCamel())
			.WithMessage($"Length must be at most {Room.NameMaxLength}");

		_ = RuleFor(x => x.Description)
			.MaximumLength(Room.DescriptionMaxLength)
			.WithErrorCode(nameof(CreateRoomRequestBody.Description).ToCamel())
			.WithMessage($"Length must be at most {Room.DescriptionMaxLength}");

		_ = RuleFor(x => x)
			.Must(x =>
				x.Name is not null ||
				x.Description is not null ||
				x.BackgroundAudioId is not null ||
				x.IsEntrance is not null)
			.WithErrorCode(nameof(CreateRoomRequestBody.Name).ToCamel())
			.WithMessage("At least one property must be provided.");
	}
}
