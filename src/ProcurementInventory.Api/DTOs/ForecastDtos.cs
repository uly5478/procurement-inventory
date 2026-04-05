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

/// <summary>月別出荷詳細 DTO（平均超過月のクリック用）</summary>
public class MonthlyShipmentDetailDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalShipped { get; set; }
    public decimal Average { get; set; }
    public bool AboveAverage { get; set; }
    public List<ShipmentTransactionDto> Transactions { get; set; } = new();
}

/// <summary>出荷トランザクション詳細 DTO</summary>
public class ShipmentTransactionDto
{
    public int Id { get; set; }
    public DateTime TransactionDate { get; set; }
    public int Quantity { get; set; }
    public string OperatorAccount { get; set; } = string.Empty;
    public string? Remark { get; set; }
}
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
