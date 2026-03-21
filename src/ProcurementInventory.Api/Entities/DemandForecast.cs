namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 需求預測結果實體
/// </summary>
public class DemandForecast
{
    public int Id { get; set; }

    /// <summary>產品 Id</summary>
    public int ProductId { get; set; }

    /// <summary>預測月份（1-12）</summary>
    public int ForecastMonth { get; set; }

    /// <summary>預測年份</summary>
    public int ForecastYear { get; set; }

    /// <summary>預測需求量（加權移動平均）</summary>
    public decimal ForecastQty { get; set; }

    /// <summary>信賴區間下界（MAX(0, 預測值 - 1.5σ)）</summary>
    public decimal ConfidenceLower { get; set; }

    /// <summary>信賴區間上界（預測值 + 1.5σ）</summary>
    public decimal ConfidenceUpper { get; set; }

    /// <summary>產生時間</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Product? Product { get; set; }
}
