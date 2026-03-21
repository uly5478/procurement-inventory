namespace ProcurementInventory.Api.DTOs;

/// <summary>需求預測結果 DTO</summary>
public class DemandForecastDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int ForecastMonth { get; set; }
    public int ForecastYear { get; set; }
    public decimal ForecastQty { get; set; }
    public decimal ConfidenceLower { get; set; }
    public decimal ConfidenceUpper { get; set; }
    public DateTime GeneratedAt { get; set; }
}

/// <summary>單一產品需求預測詳情 DTO（含歷史出貨量）</summary>
public class ProductForecastDetailDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    /// <summary>歷史每月出貨量（依時間升序）</summary>
    public List<MonthlyShipmentDto> HistoricalShipments { get; set; } = new();
    /// <summary>預測結果（下個月）</summary>
    public DemandForecastDto? Forecast { get; set; }
    /// <summary>資料不足時的錯誤訊息</summary>
    public string? ErrorMessage { get; set; }
}
