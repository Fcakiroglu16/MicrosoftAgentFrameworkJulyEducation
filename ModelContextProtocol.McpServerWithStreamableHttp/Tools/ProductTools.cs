using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using ModelContextProtocol.McpServerWithStreamableHttp.Data;
using ModelContextProtocol.McpServerWithStreamableHttp.Models;

namespace ModelContextProtocol.McpServerWithStreamableHttp.Tools;

internal class ProductTools(ProductDbContext dbContext)
{
    [McpServerTool(Name = "add_product")]
    [Description("Adds a new product with a name and price.")]
    public async Task<Product> AddProduct(
        [Description("Name of the product")] string name,
        [Description("Price of the product")] decimal price)
    {
        var product = new Product
        {
            Name = name,
            Price = price
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        return product;
    }

    [McpServerTool(Name = "list_products")]
    [Description("Lists all products.")]
    public async Task<List<Product>> ListProducts()
    {
        return await dbContext.Products.ToListAsync();
    }
}
