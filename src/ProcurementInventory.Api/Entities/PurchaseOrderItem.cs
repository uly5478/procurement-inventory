namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 採購訂單明細實體
/// </summary>
public class PurchaseOrderItem
{
    public int Id { get; set; }

    /// <summary>所屬採購訂單 Id</summary>
    public int PurchaseOrderId { get; set; }

    /// <summary>產品 Id</summary>
    public int ProductId { get; set; }

    /// <summary>採購數量</summary>
    public int Quantity { get; set; }

    /// <summary>單價</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>小計（Quantity × UnitPrice）</summary>
    public decimal Subtotal { get; set; }

    /// <summary>採購訂單導覽屬性</summary>
    public PurchaseOrder? PurchaseOrder { get; set; }

    /// <summary>產品導覽屬性</summary>
    public Product? Product { get; set; }
}
