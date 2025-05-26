using Aula.Server.Domain.Rooms;
using Aula.Server.Shared.Extensions;
using FluentValidation;

namespace Aula.Server.Features.Rooms.CreateRoom;

internal sealed class CreateRoomRequestBodyValidator : AbstractValidator<CreateRoomRequestBody>
{
	public CreateRoomRequestBodyValidator()
	{
		var allowedRoomTypes = new[] { RoomType.Standard };

		_ = RuleFor(x => x.Type)
			.IsInEnum()
			.Must(v => allowedRoomTypes.Any(allowedType => v == allowedType))
			.WithErrorCode(nameof(CreateRoomRequestBody.Type).ToCamel())
			.WithMessage($"Invalid value. Valid values are: {String.Join(", ", allowedRoomTypes.Cast<Int32>())}.");

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
	}
}
