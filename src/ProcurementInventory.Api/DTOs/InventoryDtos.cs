namespace ProcurementInventory.Api.DTOs;

/// <summary>入庫請求 DTO</summary>
public class StockInDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime TransactionDate { get; set; }
    public int? PurchaseOrderId { get; set; }
    public string? Remark { get; set; }
}

/// <summary>出貨請求 DTO</summary>
public class StockOutDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Remark { get; set; }
    /// <summary>超庫存時是否強制執行</summary>
    public bool ForceConfirm { get; set; } = false;
}

/// <summary>庫存異動結果 DTO</summary>
public class StockTransactionResultDto
{
    public int TransactionId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int StockBefore { get; set; }
    public int StockAfter { get; set; }
    public DateTime TransactionDate { get; set; }
    public string OperatorAccount { get; set; } = string.Empty;
    public string? Remark { get; set; }
    /// <summary>超庫存警告訊息（出貨時庫存不足）</summary>
    public string? Warning { get; set; }
    /// <summary>是否需要確認（超庫存時）</summary>
    public bool RequireConfirmation { get; set; }
}

/// <summary>庫存異動歷程 DTO</summary>
public class StockTransactionDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int StockBefore { get; set; }
    public int StockAfter { get; set; }
    public int? PurchaseOrderId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string OperatorAccount { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Remark { get; set; }
}

/// <summary>庫存總覽 DTO</summary>
public class InventoryOverviewDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal SixMonthAvgShipment { get; set; }
    /// <summary>庫存狀態：Normal / Low（低於安全在庫）</summary>
    public string StockStatus { get; set; } = "Normal";
    public DateTime UpdatedAt { get; set; }

    // Warehouse breakdown
    public int Warehouse89 { get; set; }
    public int Warehouse81 { get; set; }
    public int WarehouseInspection { get; set; }
    public int Warehouse4th { get; set; }
    public int TotalWarehouseStock { get; set; }
    public int UnallocatedQty { get; set; }
    public int ShippedQty { get; set; }
    public int SafetyStock { get; set; }

    /// <summary>回転月数（設定値）</summary>
    public decimal TurnoverMonths { get; set; }
    /// <summary>リードタイム月数（推奨仕入先の LeadTimeDays / 30）</summary>
    public decimal LeadTimeMonths { get; set; }

    // ForecastPage 計算用に追加
    /// <summary>有効在庫（総倉庫在庫 - 未割当数量）</summary>
    public int CurrentStock { get; set; }
    /// <summary>推奨仕入先のリードタイム（日数）</summary>
    public int? RecommendedLeadTimeDays { get; set; }
    /// <summary>最小発注数量</summary>
    public int Moq { get; set; }
    /// <summary>箱入数</summary>
    public int BoxQty { get; set; }

    /// <summary>半年分の月次発注提案（今月〜6ヶ月先）</summary>
    public List<MonthlyOrderSuggestionDto> MonthlyOrderSuggestions { get; set; } = new();
}

/// <summary>月次発注提案 DTO</summary>
public class MonthlyOrderSuggestionDto
{
    /// <summary>対象年月ラベル（例: 2026/04）</summary>
    public string Label { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    /// <summary>その月に必要な発注数（0以下は発注不要）</summary>
    public int SuggestedQty { get; set; }
    /// <summary>その月時点の推定在庫</summary>
    public int EstimatedStock { get; set; }
}

/// <summary>庫存總覽拡張 DTO（Excel エクスポート用）</summary>
public class InventoryOverviewExtendedDto : InventoryOverviewDto
{
    public decimal AverageShipment { get; set; }
    public List<SupplierInfoForExportDto> Suppliers { get; set; } = new();
    public Dictionary<int, int> MonthlyShipments { get; set; } = new(); // Month (1-12) -> Qty
}

/// <summary>Excel エクスポート用仕入先情報 DTO</summary>
public class SupplierInfoForExportDto
{
    public string SupplierName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int LeadTimeDays { get; set; }
}

/// <summary>每月出貨統計 DTO</summary>
public class MonthlyShipmentDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalShipped { get; set; }
}
