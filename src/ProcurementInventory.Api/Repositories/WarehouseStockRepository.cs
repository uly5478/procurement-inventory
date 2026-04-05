using Microsoft.EntityFrameworkCore;
using ProcurementInventory.Api.Data;
using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 倉庫別在庫 Repository 実装
/// </summary>
public class WarehouseStockRepository : IWarehouseStockRepository
{
    private readonly AppDbContext _context;

    public WarehouseStockRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WarehouseStock> GetByProductIdAsync(int productId)
    {
        var stock = await _context.WarehouseStocks
            .FirstOrDefaultAsync(w => w.ProductId == productId);

        if (stock == null)
        {
            // Auto-create with zeros if not exists
            stock = new WarehouseStock
            {
                ProductId = productId,
                Warehouse89 = 0,
                Warehouse81 = 0,
                WarehouseInspection = 0,
                Warehouse4th = 0,
                UnallocatedQty = 0,
                ShippedQty = 0,
                UpdatedAt = DateTime.UtcNow
            };
            _context.WarehouseStocks.Add(stock);
            await _context.SaveChangesAsync();
        }

        return stock;
    }

    public async Task<WarehouseStock> UpdateAsync(WarehouseStock stock)
    {
        stock.UpdatedAt = DateTime.UtcNow;
        _context.WarehouseStocks.Update(stock);
        await _context.SaveChangesAsync();
        return stock;
    }
}