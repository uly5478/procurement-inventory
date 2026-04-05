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

    /// <summary>箱入數（1箱あたりの入数）</summary>
    public int BoxQty { get; set; } = 1;

    /// <summary>最小発注数量（Minimum Order Quantity）</summary>
    public int MOQ { get; set; } = 1;

    /// <summary>安全在庫数量</summary>
    public int SafetyStock { get; set; } = 0;

    /// <summary>平均出荷数</summary>
    public decimal AverageShipment { get; set; } = 0;

    /// <summary>仕入分類コード（例: VN, CN, JP など）</summary>
    public string? CategoryCode { get; set; }

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>最後修改時間</summary>
    public DateTime? UpdatedAt { get; set; }
}
