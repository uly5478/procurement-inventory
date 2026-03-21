namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 採購訂單實體
/// </summary>
public class PurchaseOrder
{
    public int Id { get; set; }

    /// <summary>訂單編號（格式：PO-YYYYMMDD-NNNN）</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>廠商 Id</summary>
    public int SupplierId { get; set; }

    /// <summary>訂單狀態（待確認、已確認、已完成、已取消）</summary>
    public string Status { get; set; } = "待確認";

    /// <summary>訂單總金額</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>訂單日期</summary>
    public DateTime OrderDate { get; set; }

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>建立人員帳號</summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>廠商導覽屬性</summary>
    public Supplier? Supplier { get; set; }

    /// <summary>訂單明細</summary>
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
