namespace Aula.Server.Domain.Bans;

internal sealed record BanIssuedEvent(Ban Ban) : DomainEvent;
