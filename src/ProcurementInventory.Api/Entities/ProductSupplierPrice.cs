namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 產品廠商報價實體
/// </summary>
public class ProductSupplierPrice
{
    public int Id { get; set; }

    /// <summary>產品 Id（FK）</summary>
    public int ProductId { get; set; }

    /// <summary>廠商 Id（FK）</summary>
    public int SupplierId { get; set; }

    /// <summary>單價（買價）</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>幣別</summary>
    public string Currency { get; set; } = "TWD";

    /// <summary>最小訂購量</summary>
    public int MinOrderQty { get; set; }

    /// <summary>交期（天數）</summary>
    public int LeadTimeDays { get; set; }

    /// <summary>生效日期</summary>
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

    /// <summary>是否為最新報價</summary>
    public bool IsCurrent { get; set; } = true;

    /// <summary>導覽屬性：產品</summary>
    public Product? Product { get; set; }

    /// <summary>導覽屬性：廠商</summary>
    public Supplier? Supplier { get; set; }
}
