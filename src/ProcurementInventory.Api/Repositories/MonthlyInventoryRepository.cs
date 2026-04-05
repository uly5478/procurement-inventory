using Microsoft.EntityFrameworkCore;
using ProcurementInventory.Api.Data;
using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 月別在庫スナップショット Repository 実装
/// </summary>
public class MonthlyInventoryRepository : IMonthlyInventoryRepository
{
    private readonly AppDbContext _context;

    public MonthlyInventoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MonthlyInventory>> GetByProductIdAsync(int productId, int months = 12)
    {
        var now = DateTime.UtcNow;
        var cutoffDate = now.AddMonths(-months);

        return await _context.MonthlyInventories
            .Where(m => m.ProductId == productId 
                && (m.Year > cutoffDate.Year || (m.Year == cutoffDate.Year && m.Month >= cutoffDate.Month)))
            .OrderByDescending(m => m.Year)
            .ThenByDescending(m => m.Month)
            .Take(months)
            .ToListAsync();
    }

    public async Task<MonthlyInventory> RecordSnapshotAsync(MonthlyInventory inventory)
    {
        var existing = await _context.MonthlyInventories
            .FirstOrDefaultAsync(m => m.ProductId == inventory.ProductId 
                && m.Year == inventory.Year 
                && m.Month == inventory.Month);

        if (existing != null)
        {
            existing.OrderQty = inventory.OrderQty;
            existing.StockQty = inventory.StockQty;
            existing.StockAmount = inventory.StockAmount;
            existing.TurnoverRate = inventory.TurnoverRate;
            _context.MonthlyInventories.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        inventory.CreatedAt = DateTime.UtcNow;
        _context.MonthlyInventories.Add(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }
}