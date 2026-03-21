namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 庫存記錄實體（每個產品一筆，記錄當前庫存）
/// </summary>
public class InventoryRecord
{
    public int Id { get; set; }

    /// <summary>產品 Id（外鍵）</summary>
    public int ProductId { get; set; }

    /// <summary>當前庫存數量</summary>
    public int CurrentStock { get; set; }

    /// <summary>最後更新時間</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Product? Product { get; set; }
}
