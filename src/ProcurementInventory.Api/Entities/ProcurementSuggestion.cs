namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 採購建議實體（每個產品一筆）
/// </summary>
public class ProcurementSuggestion
{
    public int Id { get; set; }

    /// <summary>產品 Id（外鍵）</summary>
    public int ProductId { get; set; }

    /// <summary>六個月平均出貨量</summary>
    public decimal SixMonthAvgShipment { get; set; }

    /// <summary>使用的庫存迴轉率（月）</summary>
    public decimal TurnoverMonths { get; set; }

    /// <summary>系統計算建議採購量</summary>
    public int SystemSuggestedQty { get; set; }

    /// <summary>手動覆寫採購量（null 表示未覆寫）</summary>
    public int? ManualOverrideQty { get; set; }

    /// <summary>是否為手動覆寫</summary>
    public bool IsManualOverride { get; set; }

    /// <summary>計算時間</summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Product? Product { get; set; }
}
