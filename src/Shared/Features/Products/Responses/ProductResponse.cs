namespace Shared.Features.Products.Responses;

/// <summary>
/// Response containing product information.
/// </summary>
public record ProductResponse(
    int Id,
    string Name,
    decimal Price,
    int GroupId,
    int CategoryId,
    string CategoryName,
    string GroupName);
