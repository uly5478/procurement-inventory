using Microsoft.EntityFrameworkCore;
using ProcurementInventory.Api.Data;
using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 廠商報價 Repository 實作
/// </summary>
public class SupplierRepository : ISupplierRepository
{
    private readonly AppDbContext _db;

    public SupplierRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProductSupplierPrice>> GetCurrentPricesByProductIdAsync(int productId)
        => await _db.ProductSupplierPrices
            .Include(p => p.Supplier)
            .Where(p => p.ProductId == productId && p.IsCurrent)
            .OrderBy(p => p.UnitPrice)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<IEnumerable<ProductSupplierPrice>> GetAllPricesByProductIdAsync(int productId)
        => await _db.ProductSupplierPrices
            .Include(p => p.Supplier)
            .Where(p => p.ProductId == productId)
            .OrderByDescending(p => p.EffectiveDate)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<int> GetCurrentSupplierCountAsync(int productId)
        => await _db.ProductSupplierPrices
            .Where(p => p.ProductId == productId && p.IsCurrent)
            .Select(p => p.SupplierId)
            .Distinct()
            .CountAsync();

    /// <inheritdoc/>
    public async Task<ProductSupplierPrice> CreatePriceAsync(ProductSupplierPrice price)
    {
        _db.ProductSupplierPrices.Add(price);
        await _db.SaveChangesAsync();
        return price;
    }

    /// <inheritdoc/>
    public async Task<ProductSupplierPrice?> GetPriceByIdAsync(int id)
        => await _db.ProductSupplierPrices
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);

    /// <inheritdoc/>
    public async Task<ProductSupplierPrice> UpdatePriceAsync(ProductSupplierPrice price)
    {
        _db.ProductSupplierPrices.Update(price);
        await _db.SaveChangesAsync();
        return price;
    }

    /// <inheritdoc/>
    public async Task<Supplier?> GetSupplierByNameAsync(string name)
        => await _db.Suppliers.FirstOrDefaultAsync(s => s.Name == name);

    /// <inheritdoc/>
    public async Task<Supplier> CreateSupplierAsync(Supplier supplier)
    {
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();
        return supplier;
    }
}
