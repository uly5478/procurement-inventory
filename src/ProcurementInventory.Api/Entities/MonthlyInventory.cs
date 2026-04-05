namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 月別在庫スナップショットエンティティ
/// </summary>
public class MonthlyInventory
{
    public int Id { get; set; }

    /// <summary>商品ID</summary>
    public int ProductId { get; set; }

    /// <summary>年</summary>
    public int Year { get; set; }

    /// <summary>月（1-12）</summary>
    public int Month { get; set; }

    /// <summary>注文数量</summary>
    public int OrderQty { get; set; }

    /// <summary>在庫数量</summary>
    public int StockQty { get; set; }

    /// <summary>在庫金額</summary>
    public decimal StockAmount { get; set; }

    /// <summary>回転率</summary>
    public decimal TurnoverRate { get; set; }

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>商品ナビゲーション</summary>
    public Product? Product { get; set; }
}