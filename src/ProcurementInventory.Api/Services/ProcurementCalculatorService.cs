using ProcurementInventory.Api.Data;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Entities;
using ProcurementInventory.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 採購建議量計算 Service 實作
/// </summary>
public class ProcurementCalculatorService : IProcurementCalculatorService
{
    private readonly IProcurementRepository _repo;
    private readonly AppDbContext _db;
    private readonly IWarehouseStockRepository _warehouseStockRepo;

    public ProcurementCalculatorService(
        IProcurementRepository repo,
        AppDbContext db,
        IWarehouseStockRepository warehouseStockRepo)
    {
        _repo = repo;
        _db = db;
        _warehouseStockRepo = warehouseStockRepo;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProcurementSuggestionDto>> GetAllSuggestionsAsync(bool useForecast = false)
    {
        var settings = await _repo.GetSettingsAsync();
        var products = await _repo.GetAllProductsWithInventoryAsync();

        var results = new List<ProcurementSuggestionDto>();

        foreach (var product in products)
        {
            decimal? forecastQty = null;
            if (useForecast)
            {
                // 取得最新預測值（需求 9.5, 9.6）
                var forecast = await _db.DemandForecasts
                    .Where(f => f.ProductId == product.Id)
                    .OrderByDescending(f => f.ForecastYear)
                    .ThenByDescending(f => f.ForecastMonth)
                    .FirstOrDefaultAsync();
                forecastQty = forecast?.ForecastQty;
            }

            var dto = await CalculateSuggestionAsync(product, settings.DefaultTurnoverMonths, forecastQty);
            results.Add(dto);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<ProcurementSuggestionDto?> GetSuggestionByProductIdAsync(int productId)
    {
        var settings = await _repo.GetSettingsAsync();
        var products = await _repo.GetAllProductsWithInventoryAsync();
        var product = products.FirstOrDefault(p => p.Id == productId);

        if (product is null)
            return null;

        return await CalculateSuggestionAsync(product, settings.DefaultTurnoverMonths);
    }

    /// <inheritdoc/>
    public async Task<ProcurementSuggestionDto> ManualOverrideAsync(int productId, int qty)
    {
        // 先計算系統建議值（確保 suggestion 存在）
        var settings = await _repo.GetSettingsAsync();
        var products = await _repo.GetAllProductsWithInventoryAsync();
        var product = products.FirstOrDefault(p => p.Id == productId)
            ?? throw new KeyNotFoundException($"找不到產品 Id={productId}");

        var calcDto = await CalculateSuggestionAsync(product, settings.DefaultTurnoverMonths);

        // 取得或建立 suggestion 實體
        var existing = await _repo.GetSuggestionByProductIdAsync(productId);
        var suggestion = existing ?? new ProcurementSuggestion
        {
            ProductId = productId,
            SixMonthAvgShipment = calcDto.SixMonthAvgShipment,
            TurnoverMonths = calcDto.TurnoverMonths,
            SystemSuggestedQty = calcDto.SystemSuggestedQty,
            CalculatedAt = calcDto.CalculatedAt
        };

        // 套用手動覆寫
        suggestion.ManualOverrideQty = qty;
        suggestion.IsManualOverride = true;
        suggestion.CalculatedAt = DateTime.UtcNow;

        await _repo.UpsertSuggestionAsync(suggestion);

        calcDto.ManualOverrideQty = qty;
        calcDto.IsManualOverride = true;
        calcDto.CalculatedAt = suggestion.CalculatedAt;
        return calcDto;
    }

    /// <inheritdoc/>
    public async Task<ProcurementSuggestionDto> ResetOverrideAsync(int productId)
    {
        var existing = await _repo.GetSuggestionByProductIdAsync(productId);
        if (existing != null)
        {
            existing.ManualOverrideQty = null;
            existing.IsManualOverride = false;
            existing.CalculatedAt = DateTime.UtcNow;
            await _repo.UpsertSuggestionAsync(existing);
        }

        var settings = await _repo.GetSettingsAsync();
        var products = await _repo.GetAllProductsWithInventoryAsync();
        var product = products.FirstOrDefault(p => p.Id == productId)
            ?? throw new KeyNotFoundException($"找不到產品 Id={productId}");

        return await CalculateSuggestionAsync(product, settings.DefaultTurnoverMonths);
    }

    /// <inheritdoc/>
    public async Task<ProcurementSettingsDto> GetSettingsAsync()
    {
        var settings = await _repo.GetSettingsAsync();
        return MapSettingsToDto(settings);
    }

    /// <inheritdoc/>
    public async Task<ProcurementSettingsDto> UpdateSettingsAsync(UpdateProcurementSettingsDto dto)
    {
        if (dto.DefaultTurnoverMonths < 1.0m || dto.DefaultTurnoverMonths > 6.0m)
            throw new ArgumentException("庫存迴轉率必須介於 1.0 至 6.0 個月之間");

        var settings = await _repo.GetSettingsAsync();
        settings.DefaultTurnoverMonths = dto.DefaultTurnoverMonths;
        settings.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.UpdateSettingsAsync(settings);
        return MapSettingsToDto(updated);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<ProcurementSuggestionDto> CalculateSuggestionAsync(
        Entities.Product product, decimal turnoverMonths, decimal? forecastAvg = null)
    {
        decimal avg;
        bool dataInsufficient = false;
        int? availableMonths = null;

        if (forecastAvg.HasValue)
        {
            avg = forecastAvg.Value;
        }
        else if (product.AverageShipment > 0)
        {
            // 商品マスタの平均出荷数を優先（在庫一覧と同じ）
            avg = product.AverageShipment;
        }
        else
        {
            // フォールバック: MonthlyShipments テーブルから計算
            var monthlyData = await _repo.GetMonthlyShipmentAsync(product.Id, 6);

            if (monthlyData.Count == 0)
            {
                avg = 0m;
            }
            else if (monthlyData.Count >= 6)
            {
                avg = (decimal)monthlyData.Sum(m => m.TotalQty) / 6m;
            }
            else
            {
                avg = (decimal)monthlyData.Sum(m => m.TotalQty) / monthlyData.Count;
                dataInsufficient = true;
                availableMonths = monthlyData.Count;
            }
        }

        // 在庫合計 = 倉庫合計（在庫一覧と同じ）
        var suggestion = await _repo.GetSuggestionByProductIdAsync(product.Id);
        var warehouseStock = await _warehouseStockRepo.GetByProductIdAsync(product.Id);
        int totalWarehouseStock = warehouseStock.Warehouse89 + warehouseStock.Warehouse81
            + warehouseStock.WarehouseInspection + warehouseStock.Warehouse4th;

        // 推奨仕入先（最安値）のリードタイム月数
        var topSuppliers = await _db.ProductSupplierPrices
            .Include(p => p.Supplier)
            .Where(p => p.ProductId == product.Id && p.IsCurrent)
            .OrderBy(p => p.UnitPrice)
            .Take(2)
            .ToListAsync();

        var supplier1 = topSuppliers.Count > 0 ? topSuppliers[0] : null;
        var supplier2 = topSuppliers.Count > 1 ? topSuppliers[1] : null;
        decimal leadTimeMonths = supplier1 != null ? supplier1.LeadTimeDays / 30m : 0m;

        // 公式（在庫一覧と統一）:
        // 発注数 = 平均出荷数 × (回転月数 + リードタイム月数) - (倉庫合計 - 未引当数量)
        int effectiveStock = totalWarehouseStock - warehouseStock.UnallocatedQty;
        decimal needed = avg * (turnoverMonths + leadTimeMonths) - effectiveStock;
        int baseQty = needed > 0 ? (int)Math.Ceiling(needed) : 0;

        // MOQ 丸め
        if (baseQty > 0 && baseQty < product.MOQ)
            baseQty = product.MOQ;

        // BoxQty 丸め
        if (baseQty > 0 && product.BoxQty > 1 && baseQty % product.BoxQty != 0)
            baseQty = (baseQty / product.BoxQty + 1) * product.BoxQty;

        int suggestedQty = baseQty;

        // 60:40 split — BoxQty丸め後に合計が suggestedQty と一致するよう調整
        int? s1OrderQty = null;
        int? s2OrderQty = null;
        if (suggestedQty > 0)
        {
            int bq = product.BoxQty > 0 ? product.BoxQty : 1;
            // 第1仕入先: 60% を BoxQty 単位で切り上げ
            int raw1 = (int)Math.Ceiling(suggestedQty * 0.6 / bq) * bq;
            // 第2仕入先: 合計が suggestedQty になるよう残りを BoxQty 単位で切り上げ
            int raw2 = suggestedQty - raw1;
            if (raw2 < 0) raw2 = 0;
            int s2 = raw2 > 0 ? (int)Math.Ceiling((double)raw2 / bq) * bq : 0;

            s1OrderQty = raw1;
            s2OrderQty = supplier2 != null ? s2 : 0;
        }

        // 半年分の月次発注提案（在庫一覧と完全に同じロジック）
        // avg は商品マスタの AverageShipment を優先（在庫一覧と同じ）
        decimal avgForMonthly = product.AverageShipment > 0 ? product.AverageShipment : avg;
        var monthlyOrderSuggestions = new List<MonthlyOrderSuggestionDto>();
        decimal estimatedStock = effectiveStock;  // 有効在庫から開始
        var now = DateTime.UtcNow;

        for (int i = 0; i < 6; i++)
        {
            var targetDate = now.AddMonths(i);
            if (i > 0) estimatedStock = Math.Max(0, estimatedStock - avgForMonthly);

            decimal needed2 = avgForMonthly * (turnoverMonths + leadTimeMonths) - estimatedStock;
            int mQty = needed2 > 0 ? (int)Math.Ceiling(needed2) : 0;
            int bq2 = product.BoxQty > 0 ? product.BoxQty : 1;
            if (mQty > 0)
            {
                if (mQty < product.MOQ) mQty = product.MOQ;
                if (bq2 > 1 && mQty % bq2 != 0) mQty = (mQty / bq2 + 1) * bq2;
            }

            monthlyOrderSuggestions.Add(new MonthlyOrderSuggestionDto
            {
                Label = $"{targetDate.Year}/{targetDate.Month:D2}",
                Year = targetDate.Year,
                Month = targetDate.Month,
                SuggestedQty = mQty,
                EstimatedStock = (int)estimatedStock,
            });

            if (mQty > 0) estimatedStock += mQty;
        }

        return new ProcurementSuggestionDto
        {
            ProductId = product.Id,
            ProductCode = product.ProductCode,
            ProductName = product.Name,
            CurrentStock = totalWarehouseStock,
            SixMonthAvgShipment = avg,
            TurnoverMonths = turnoverMonths,
            SystemSuggestedQty = suggestedQty,
            ManualOverrideQty = suggestion?.ManualOverrideQty,
            IsManualOverride = suggestion?.IsManualOverride ?? false,
            DataInsufficient = dataInsufficient,
            AvailableMonths = availableMonths,
            CalculatedAt = now,
            BoxQty = product.BoxQty,
            MOQ = product.MOQ,
            SafetyStock = product.SafetyStock,
            RecommendedSupplierName = supplier1?.Supplier?.Name,
            RecommendedUnitPrice = supplier1?.UnitPrice,
            RecommendedCurrency = supplier1?.Currency,
            RecommendedLeadTimeDays = supplier1?.LeadTimeDays,
            Supplier1OrderQty = s1OrderQty,
            Supplier2Name = supplier2?.Supplier?.Name,
            Supplier2UnitPrice = supplier2?.UnitPrice,
            Supplier2Currency = supplier2?.Currency,
            Supplier2LeadTimeDays = supplier2?.LeadTimeDays,
            Supplier2OrderQty = s2OrderQty,
            NoSupplier = supplier1 == null,
            MonthlyOrderSuggestions = monthlyOrderSuggestions,
        };
    }

    private static ProcurementSettingsDto MapSettingsToDto(ProcurementSettings settings)
        => new()
        {
            Id = settings.Id,
            DefaultTurnoverMonths = settings.DefaultTurnoverMonths,
            UpdatedBy = settings.UpdatedBy,
            UpdatedAt = settings.UpdatedAt
        };
}
