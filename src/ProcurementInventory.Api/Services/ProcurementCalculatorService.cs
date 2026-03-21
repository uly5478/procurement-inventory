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

    public ProcurementCalculatorService(IProcurementRepository repo, AppDbContext db)
    {
        _repo = repo;
        _db = db;
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
            // 預測輔助模式：以預測值取代六個月平均出貨量（需求 9.6）
            avg = forecastAvg.Value;
        }
        else
        {
            // 取最近 6 個月出貨記錄（按月分組）
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

        // 建議採購量 = MAX(0, ROUND(avg × rate) - currentStock)
        var suggestion = await _repo.GetSuggestionByProductIdAsync(product.Id);
        int currentStock = await _repo.GetCurrentStockAsync(product.Id);
        int suggestedQty = Math.Max(0, (int)Math.Round(avg * turnoverMonths) - currentStock);

        var now = DateTime.UtcNow;

        return new ProcurementSuggestionDto
        {
            ProductId = product.Id,
            ProductCode = product.ProductCode,
            ProductName = product.Name,
            CurrentStock = currentStock,
            SixMonthAvgShipment = avg,
            TurnoverMonths = turnoverMonths,
            SystemSuggestedQty = suggestedQty,
            ManualOverrideQty = suggestion?.ManualOverrideQty,
            IsManualOverride = suggestion?.IsManualOverride ?? false,
            DataInsufficient = dataInsufficient,
            AvailableMonths = availableMonths,
            CalculatedAt = now
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
