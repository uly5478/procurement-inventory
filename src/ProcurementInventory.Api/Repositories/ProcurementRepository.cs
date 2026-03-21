using Microsoft.EntityFrameworkCore;
using ProcurementInventory.Api.Data;
using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 採購計算器 Repository 實作
/// </summary>
public class ProcurementRepository : IProcurementRepository
{
    private readonly AppDbContext _db;

    public ProcurementRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProcurementSuggestion>> GetAllSuggestionsAsync()
        => await _db.ProcurementSuggestions
            .Include(s => s.Product)
            .OrderBy(s => s.Product!.ProductCode)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<ProcurementSuggestion?> GetSuggestionByProductIdAsync(int productId)
        => await _db.ProcurementSuggestions
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.ProductId == productId);

    /// <inheritdoc/>
    public async Task<ProcurementSuggestion> UpsertSuggestionAsync(ProcurementSuggestion suggestion)
    {
        var existing = await _db.ProcurementSuggestions
            .FirstOrDefaultAsync(s => s.ProductId == suggestion.ProductId);

        if (existing is null)
        {
            _db.ProcurementSuggestions.Add(suggestion);
        }
        else
        {
            existing.SixMonthAvgShipment = suggestion.SixMonthAvgShipment;
            existing.TurnoverMonths = suggestion.TurnoverMonths;
            existing.SystemSuggestedQty = suggestion.SystemSuggestedQty;
            existing.ManualOverrideQty = suggestion.ManualOverrideQty;
            existing.IsManualOverride = suggestion.IsManualOverride;
            existing.CalculatedAt = suggestion.CalculatedAt;
            suggestion = existing;
        }

        await _db.SaveChangesAsync();
        return suggestion;
    }

    /// <inheritdoc/>
    public async Task<ProcurementSettings> GetSettingsAsync()
    {
        var settings = await _db.ProcurementSettings.FirstOrDefaultAsync();
        if (settings is null)
        {
            // 回傳預設值（不儲存至資料庫）
            return new ProcurementSettings
            {
                Id = 0,
                DefaultTurnoverMonths = 2.5m,
                UpdatedBy = string.Empty,
                UpdatedAt = DateTime.UtcNow
            };
        }
        return settings;
    }

    /// <inheritdoc/>
    public async Task<ProcurementSettings> UpdateSettingsAsync(ProcurementSettings settings)
    {
        var existing = await _db.ProcurementSettings.FirstOrDefaultAsync();
        if (existing is null)
        {
            _db.ProcurementSettings.Add(settings);
        }
        else
        {
            existing.DefaultTurnoverMonths = settings.DefaultTurnoverMonths;
            existing.UpdatedBy = settings.UpdatedBy;
            existing.UpdatedAt = settings.UpdatedAt;
            settings = existing;
        }

        await _db.SaveChangesAsync();
        return settings;
    }

    /// <inheritdoc/>
    public async Task<List<(int Year, int Month, int TotalQty)>> GetMonthlyShipmentAsync(int productId, int months)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-months);

        var result = await _db.StockTransactions
            .Where(t => t.ProductId == productId
                     && t.TransactionType == "出貨"
                     && t.TransactionDate >= cutoff)
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                TotalQty = g.Sum(t => t.Quantity)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();

        return result.Select(x => (x.Year, x.Month, x.TotalQty)).ToList();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Entities.Product>> GetAllProductsWithInventoryAsync()
        => await _db.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProductCode)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<int> GetCurrentStockAsync(int productId)
    {
        var record = await _db.InventoryRecords
            .FirstOrDefaultAsync(r => r.ProductId == productId);
        return record?.CurrentStock ?? 0;
    }
}
