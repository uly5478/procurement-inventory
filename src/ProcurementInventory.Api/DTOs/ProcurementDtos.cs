namespace ProcurementInventory.Api.DTOs;

/// <summary>
/// 採購建議 DTO
/// </summary>
public class ProcurementSuggestionDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public decimal SixMonthAvgShipment { get; set; }
    public decimal TurnoverMonths { get; set; }
    public int SystemSuggestedQty { get; set; }
    public int? ManualOverrideQty { get; set; }
    public bool IsManualOverride { get; set; }

    /// <summary>出貨記錄不足 6 個月時為 true</summary>
    public bool DataInsufficient { get; set; }

    /// <summary>實際可用的出貨月份數（不足 6 個月時填入）</summary>
    public int? AvailableMonths { get; set; }

    public DateTime CalculatedAt { get; set; }

    // NEW: Procurement fields
    public int BoxQty { get; set; }
    public int MOQ { get; set; }
    public int SafetyStock { get; set; }

    // NEW: Recommended supplier info (1st cheapest)
    public string? RecommendedSupplierName { get; set; }
    public decimal? RecommendedUnitPrice { get; set; }
    public string? RecommendedCurrency { get; set; }
    public int? RecommendedLeadTimeDays { get; set; }
    /// <summary>第1仕入先への発注数（60%、箱入数丸め済み）</summary>
    public int? Supplier1OrderQty { get; set; }

    // 2nd cheapest supplier
    public string? Supplier2Name { get; set; }
    public decimal? Supplier2UnitPrice { get; set; }
    public string? Supplier2Currency { get; set; }
    public int? Supplier2LeadTimeDays { get; set; }
    /// <summary>第2仕入先への発注数（40%、箱入数丸め済み）</summary>
    public int? Supplier2OrderQty { get; set; }

    public bool NoSupplier { get; set; }

    /// <summary>半年分の月次発注提案（在庫一覧と同じロジック）</summary>
    public List<MonthlyOrderSuggestionDto> MonthlyOrderSuggestions { get; set; } = new();
}

/// <summary>
/// 採購設定 DTO
/// </summary>
public class ProcurementSettingsDto
{
    public int Id { get; set; }
    public decimal DefaultTurnoverMonths { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 更新採購設定 DTO
/// </summary>
public class UpdateProcurementSettingsDto
{
    /// <summary>庫存迴轉率（月），範圍 1.0–6.0</summary>
    public decimal DefaultTurnoverMonths { get; set; }
}

/// <summary>
/// 手動覆寫採購量 DTO
/// </summary>
public class ManualOverrideDto
{
    /// <summary>手動覆寫的採購量（必須 >= 0）</summary>
    public int ManualOverrideQty { get; set; }
}
