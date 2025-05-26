namespace Aula.Server.Domain.Bans;

internal sealed record BanLiftedEvent(Ban Ban) : DomainEvent;
