using Server.Domain.Common;

namespace Server.Domain;

public class Category : IAuditableEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ProductGroup> Groups { get; set; } = new List<ProductGroup>();
}
