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
    public int CurrentStock { get; set; }
    public decimal SixMonthAvgShipment { get; set; }
    public int SuggestedProcurementQty { get; set; }
    /// <summary>庫存狀態：Normal / Low（低於六個月平均出貨量）</summary>
    public string StockStatus { get; set; } = "Normal";
    public DateTime UpdatedAt { get; set; }
}

/// <summary>每月出貨統計 DTO</summary>
public class MonthlyShipmentDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalShipped { get; set; }
}
