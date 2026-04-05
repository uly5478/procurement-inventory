namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 倉庫別在庫エンティティ
/// </summary>
public class WarehouseStock
{
    public int Id { get; set; }

    /// <summary>商品ID</summary>
    public int ProductId { get; set; }

    /// <summary>89倉庫在庫数量</summary>
    public int Warehouse89 { get; set; } = 0;

    /// <summary>81倉庫在庫数量</summary>
    public int Warehouse81 { get; set; } = 0;

    /// <summary>検査倉庫在庫数量</summary>
    public int WarehouseInspection { get; set; } = 0;

    /// <summary>第四倉庫在庫数量</summary>
    public int Warehouse4th { get; set; } = 0;

    /// <summary>未引当数量</summary>
    public int UnallocatedQty { get; set; } = 0;

    /// <summary>出荷数（累計）</summary>
    public int ShippedQty { get; set; } = 0;

    /// <summary>最終更新日時</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>商品ナビゲーション</summary>
    public Product? Product { get; set; }
}