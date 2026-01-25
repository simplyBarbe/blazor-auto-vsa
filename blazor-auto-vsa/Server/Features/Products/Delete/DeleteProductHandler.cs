using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Products.Delete;

namespace Server.Features.Products.Delete;

/// <summary>
/// Handler for DeleteProductCommand - deletes a product.
/// </summary>
public class DeleteProductHandler : DeleteEntityHandlerBase<Product, DeleteProductCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteProductHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeleteProductHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
    }
}
