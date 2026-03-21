using Microsoft.EntityFrameworkCore;
using ProcurementInventory.Api.Data;
using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 庫存 Repository 實作
/// </summary>
public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _db;

    public InventoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<InventoryRecord?> GetByProductIdAsync(int productId)
        => await _db.InventoryRecords
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.ProductId == productId);

    public async Task<IEnumerable<InventoryRecord>> GetAllAsync()
        => await _db.InventoryRecords
            .Include(r => r.Product)
            .OrderBy(r => r.Product!.ProductCode)
            .ToListAsync();

    public async Task<InventoryRecord> CreateAsync(InventoryRecord record)
    {
        _db.InventoryRecords.Add(record);
        await _db.SaveChangesAsync();
        return record;
    }

    public async Task UpdateAsync(InventoryRecord record)
    {
        _db.InventoryRecords.Update(record);
        await _db.SaveChangesAsync();
    }

    public async Task<StockTransaction> AddTransactionAsync(StockTransaction transaction)
    {
        _db.StockTransactions.Add(transaction);
        await _db.SaveChangesAsync();
        return transaction;
    }

    public async Task<IEnumerable<StockTransaction>> GetTransactionsByProductAsync(
        int productId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var q = _db.StockTransactions
            .Where(t => t.ProductId == productId)
            .AsQueryable();

        if (startDate.HasValue)
            q = q.Where(t => t.TransactionDate >= startDate.Value);
        if (endDate.HasValue)
            q = q.Where(t => t.TransactionDate <= endDate.Value);

        return await q.OrderBy(t => t.TransactionDate).ToListAsync();
    }

    public async Task<IEnumerable<MonthlyShipmentSummary>> GetMonthlyShipmentSummaryAsync(
        int productId, int months = 6)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-months);
        var transactions = await _db.StockTransactions
            .Where(t => t.ProductId == productId
                     && t.TransactionType == "出貨"
                     && t.TransactionDate >= cutoff)
            .ToListAsync();

        return transactions
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new MonthlyShipmentSummary
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalShipped = g.Sum(t => t.Quantity)
            })
            .OrderBy(s => s.Year).ThenBy(s => s.Month)
            .ToList();
    }
}
