namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 採購設定實體（全域設定，通常只有一筆）
/// </summary>
public class ProcurementSettings
{
    public int Id { get; set; }

    /// <summary>預設庫存迴轉率（月），範圍 1.0–6.0，預設 2.5</summary>
    public decimal DefaultTurnoverMonths { get; set; } = 2.5m;

    /// <summary>最後更新人員</summary>
    public string UpdatedBy { get; set; } = string.Empty;

    /// <summary>最後更新時間</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
