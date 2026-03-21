namespace ProcurementInventory.Api.Entities;

/// <summary>
/// 廠商實體
/// </summary>
public class Supplier
{
    public int Id { get; set; }

    /// <summary>廠商名稱</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>聯絡資訊</summary>
    public string? ContactInfo { get; set; }

    /// <summary>廠商報價清單</summary>
    public ICollection<ProductSupplierPrice> Prices { get; set; } = new List<ProductSupplierPrice>();
}
