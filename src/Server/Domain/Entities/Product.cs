using Server.Domain.Common;

namespace Server.Domain;

public class Product : IAuditableEntity
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public ProductGroup Group { get; set; } = null!;
}