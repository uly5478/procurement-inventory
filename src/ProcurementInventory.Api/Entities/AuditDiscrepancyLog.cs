namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 帳實不符稽核記錄實體
/// </summary>
public class AuditDiscrepancyLog
{
    public int Id { get; set; }

    /// <summary>產品 Id</summary>
    public int ProductId { get; set; }

    /// <summary>帳面庫存（由 StockTransaction 累計）</summary>
    public int BookStock { get; set; }

    /// <summary>實際庫存（InventoryRecord.CurrentStock）</summary>
    public int ActualStock { get; set; }

    /// <summary>差異數量（BookStock - ActualStock）</summary>
    public int Discrepancy { get; set; }

    /// <summary>稽核時間</summary>
    public DateTime AuditedAt { get; set; } = DateTime.UtcNow;

    /// <summary>是否已發送通知</summary>
    public bool NotificationSent { get; set; }

    // Navigation
    public Product? Product { get; set; }
}
