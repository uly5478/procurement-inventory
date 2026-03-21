namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 庫存異動記錄實體
/// </summary>
public class StockTransaction
{
    public int Id { get; set; }

    /// <summary>產品 Id（外鍵）</summary>
    public int ProductId { get; set; }

    /// <summary>異動類型（入庫 / 出貨）</summary>
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>異動數量</summary>
    public int Quantity { get; set; }

    /// <summary>異動前庫存</summary>
    public int StockBefore { get; set; }

    /// <summary>異動後庫存</summary>
    public int StockAfter { get; set; }

    /// <summary>對應採購訂單 Id（可為 null）</summary>
    public int? PurchaseOrderId { get; set; }

    /// <summary>異動日期</summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>操作人員帳號</summary>
    public string OperatorAccount { get; set; } = string.Empty;

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>備註</summary>
    public string? Remark { get; set; }

    // Navigation
    public Product? Product { get; set; }
}
