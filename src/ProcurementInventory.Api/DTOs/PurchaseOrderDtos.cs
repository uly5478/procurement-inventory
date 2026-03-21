namespace ProcurementInventory.Api.DTOs;

/// <summary>採購訂單明細 DTO（查詢用）</summary>
public class PurchaseOrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

/// <summary>採購訂單 DTO（查詢用）</summary>
public class PurchaseOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}

/// <summary>建立採購訂單明細 DTO</summary>
public class CreatePurchaseOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>建立採購訂單 DTO</summary>
public class CreatePurchaseOrderDto
{
    public int SupplierId { get; set; }
    public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();
}

/// <summary>採購訂單查詢篩選 DTO</summary>
public class PurchaseOrderQueryDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SupplierName { get; set; }
    public string? Status { get; set; }
}
