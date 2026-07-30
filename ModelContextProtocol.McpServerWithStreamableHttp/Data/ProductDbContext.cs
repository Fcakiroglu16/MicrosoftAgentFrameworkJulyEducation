using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.McpServerWithStreamableHttp.Models;

namespace ModelContextProtocol.McpServerWithStreamableHttp.Data;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}
