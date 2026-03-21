using Microsoft.EntityFrameworkCore;
using ProcurementInventory.Api.Data;
using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 產品 Repository 實作
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Product>> GetAllAsync(string? keyword, bool? isActive)
    {
        var query = _db.Products.AsQueryable();

        // isActive 篩選：null = 全部，true = 只回傳啟用，false = 只回傳停用
        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }
        // isActive=null 時不加篩選，回傳全部

        // 關鍵字模糊搜尋（ProductCode 或 Name，不區分大小寫）
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lower = keyword.ToLower();
            query = query.Where(p =>
                p.ProductCode.ToLower().Contains(lower) ||
                p.Name.ToLower().Contains(lower));
        }

        return await query.OrderBy(p => p.ProductCode).ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Product?> GetByIdAsync(int id)
        => await _db.Products.FindAsync(id);

    /// <inheritdoc/>
    public async Task<Product?> GetByCodeAsync(string productCode)
        => await _db.Products.FirstOrDefaultAsync(p => p.ProductCode == productCode);

    /// <inheritdoc/>
    public async Task<Product> CreateAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    /// <inheritdoc/>
    public async Task<Product> UpdateAsync(Product product)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
        return product;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string productCode, int? excludeId = null)
    {
        var query = _db.Products.Where(p => p.ProductCode == productCode);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}
