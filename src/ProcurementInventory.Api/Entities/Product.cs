namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 產品基本資料實體
/// </summary>
public class Product
{
    public int Id { get; set; }

    /// <summary>產品編號（唯一）</summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>產品名稱</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>單位</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>是否啟用</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>最後修改時間</summary>
    public DateTime? UpdatedAt { get; set; }
}
