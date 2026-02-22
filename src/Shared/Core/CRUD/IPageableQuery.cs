namespace Shared.Core.CRUD;

public interface IPageableQuery
{
    int? PageNumber { get; set; }
    int? PageSize { get; set; }
}
