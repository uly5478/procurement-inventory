namespace ProcurementInventory.Api.DTOs;

/// <summary>廠商報價回應 DTO</summary>
public class SupplierPriceDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int MinOrderQty { get; set; }
    public int LeadTimeDays { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool IsCurrent { get; set; }
}

/// <summary>新增廠商報價 DTO</summary>
public class CreateSupplierPriceDto
{
    public string SupplierName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "TWD";
    public int MinOrderQty { get; set; }
    public int LeadTimeDays { get; set; }

    /// <summary>第 5 家廠商時，是否強制新增（跳過警告）</summary>
    public bool ForceCreate { get; set; } = false;
}

/// <summary>更新廠商報價 DTO</summary>
public class UpdateSupplierPriceDto
{
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "TWD";
    public int MinOrderQty { get; set; }
    public int LeadTimeDays { get; set; }
}

/// <summary>廠商報價清單結果（含警告資訊）</summary>
public class SupplierPriceListResult
{
    public List<SupplierPriceDto> Items { get; set; } = new();

    /// <summary>警告訊息（如第 5 家廠商警告）</summary>
    public string? Warning { get; set; }

    /// <summary>是否需要使用者確認</summary>
    public bool RequireConfirmation { get; set; }
}
