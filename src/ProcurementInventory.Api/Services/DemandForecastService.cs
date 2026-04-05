using Microsoft.EntityFrameworkCore;
using ProcurementInventory.Api.Data;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 需求預測 Service 實作（需求 9.1, 9.3, 9.4, 9.8）
/// 使用加權移動平均（WMA）演算法，線性遞增權重
/// </summary>
public class DemandForecastService : IDemandForecastService
{
    private readonly AppDbContext _db;
    private const int MinDataMonths = 3;

    public DemandForecastService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DemandForecastDto>> GetAllForecastsAsync()
    {
        var products = await _db.Products.Where(p => p.IsActive).ToListAsync();
        var results = new List<DemandForecastDto>();

        foreach (var product in products)
        {
            var monthly = await GetMonthlyShipmentsAsync(product.Id, 12);
            if (monthly.Count < MinDataMonths) continue;

            var forecast = CalculateWmaForecast(product.Id, monthly);
            await UpsertForecastAsync(product.Id, forecast);

            results.Add(new DemandForecastDto
            {
                ProductId = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.Name,
                ForecastMonth = forecast.ForecastMonth,
                ForecastYear = forecast.ForecastYear,
                ForecastQty = forecast.ForecastQty,
                ConfidenceLower = forecast.ConfidenceLower,
                ConfidenceUpper = forecast.ConfidenceUpper,
                GeneratedAt = forecast.GeneratedAt
            });
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<ProductForecastDetailDto> GetProductForecastAsync(int productId)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product is null)
            throw new ArgumentException($"找不到產品 Id={productId}");

        var monthly = await GetMonthlyShipmentsAsync(productId, 12);

        var detail = new ProductForecastDetailDto
        {
            ProductId = productId,
            ProductCode = product.ProductCode,
            ProductName = product.Name,
            HistoricalShipments = monthly.Select(m => new MonthlyShipmentDto
            {
                Year = m.Year,
                Month = m.Month,
                TotalShipped = m.TotalShipped
            }).ToList()
        };

        if (monthly.Count < MinDataMonths)
        {
            detail.ErrorMessage = $"資料不足：需要至少 {MinDataMonths} 個月的出貨記錄，目前僅有 {monthly.Count} 個月";
            return detail;
        }

        var forecast = CalculateWmaForecast(productId, monthly);
        await UpsertForecastAsync(productId, forecast);

        detail.Forecast = new DemandForecastDto
        {
            ProductId = productId,
            ProductCode = product.ProductCode,
            ProductName = product.Name,
            ForecastMonth = forecast.ForecastMonth,
            ForecastYear = forecast.ForecastYear,
            ForecastQty = forecast.ForecastQty,
            ConfidenceLower = forecast.ConfidenceLower,
            ConfidenceUpper = forecast.ConfidenceUpper,
            GeneratedAt = forecast.GeneratedAt
        };

        return detail;
    }

    /// <summary>
    /// 加權移動平均（WMA）計算，線性遞增權重（最近月份權重最高）
    /// </summary>
    private static DemandForecast CalculateWmaForecast(int productId, List<(int Year, int Month, int TotalShipped)> monthly)
    {
        int n = monthly.Count;
        // 線性遞增權重：1, 2, 3, ..., n
        double weightSum = n * (n + 1) / 2.0;
        double wma = 0;
        for (int i = 0; i < n; i++)
        {
            double weight = i + 1; // 最舊的月份權重 1，最新的月份權重 n
            wma += monthly[i].TotalShipped * weight / weightSum;
        }

        // 計算標準差
        double mean = monthly.Average(m => (double)m.TotalShipped);
        double variance = monthly.Sum(m => Math.Pow(m.TotalShipped - mean, 2)) / n;
        double sigma = Math.Sqrt(variance);

        decimal forecastQty = Math.Round((decimal)wma, 0);
        decimal lower = Math.Max(0, forecastQty - (decimal)(1.5 * sigma));
        decimal upper = forecastQty + (decimal)(1.5 * sigma);

        // 預測下個月
        var lastMonth = monthly[^1];
        var nextMonth = lastMonth.Month == 12
            ? (Year: lastMonth.Year + 1, Month: 1)
            : (Year: lastMonth.Year, Month: lastMonth.Month + 1);

        return new DemandForecast
        {
            ProductId = productId,
            ForecastMonth = nextMonth.Month,
            ForecastYear = nextMonth.Year,
            ForecastQty = forecastQty,
            ConfidenceLower = Math.Round(lower, 0),
            ConfidenceUpper = Math.Round(upper, 0),
            GeneratedAt = DateTime.UtcNow
        };
    }

    private async Task<List<(int Year, int Month, int TotalShipped)>> GetMonthlyShipmentsAsync(
        int productId, int months)
    {
        // MonthlyShipments テーブルから取得（より正確）
        var shipments = await _db.MonthlyShipments
            .Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
            .Take(months)
            .ToListAsync();

        if (shipments.Count > 0)
        {
            return shipments
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .Select(m => (m.Year, m.Month, m.Quantity))
                .ToList();
        }

        // フォールバック: StockTransactions から集計（期間制限なし）
        var transactions = await _db.StockTransactions
            .Where(t => t.ProductId == productId && t.TransactionType == "出荷")
            .ToListAsync();

        return transactions
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => (g.Key.Year, g.Key.Month, g.Sum(t => t.Quantity)))
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .TakeLast(months)
            .ToList();
    }

    private async Task UpsertForecastAsync(int productId, DemandForecast forecast)
    {
        var existing = await _db.DemandForecasts
            .FirstOrDefaultAsync(f => f.ProductId == productId
                                   && f.ForecastYear == forecast.ForecastYear
                                   && f.ForecastMonth == forecast.ForecastMonth);

        if (existing is null)
        {
            _db.DemandForecasts.Add(forecast);
        }
        else
        {
            existing.ForecastQty = forecast.ForecastQty;
            existing.ConfidenceLower = forecast.ConfidenceLower;
            existing.ConfidenceUpper = forecast.ConfidenceUpper;
            existing.GeneratedAt = forecast.GeneratedAt;
        }

        await _db.SaveChangesAsync();
    }
}
