namespace Server.Domain.Common;

public interface ISoftDeletableEntity
{
    bool Enabled { get; set; }
}