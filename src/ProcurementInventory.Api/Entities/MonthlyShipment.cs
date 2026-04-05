namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 月別出荷実績エンティティ
/// </summary>
public class MonthlyShipment
{
    public int Id { get; set; }

    /// <summary>商品ID</summary>
    public int ProductId { get; set; }

    /// <summary>年</summary>
    public int Year { get; set; }

    /// <summary>月（1-12）</summary>
    public int Month { get; set; }

    /// <summary>出荷数量</summary>
    public int Quantity { get; set; }

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>最後修改時間</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>商品ナビゲーション</summary>
    public Product? Product { get; set; }
}