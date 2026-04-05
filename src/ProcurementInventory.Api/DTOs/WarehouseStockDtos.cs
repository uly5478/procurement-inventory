using System.ComponentModel.DataAnnotations;

namespace ProcurementInventory.Api.DTOs;

/// <summary>倉庫別在庫 DTO</summary>
public class WarehouseStockDto
{
    public int ProductId { get; set; }
    public int Warehouse89 { get; set; }
    public int Warehouse81 { get; set; }
    public int WarehouseInspection { get; set; }
    public int Warehouse4th { get; set; }
    public int TotalStock { get; set; }
    public int UnallocatedQty { get; set; }
    public int ShippedQty { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>倉庫別在庫更新 DTO</summary>
public class UpdateWarehouseStockDto
{
    [Range(0, int.MaxValue, ErrorMessage = "倉庫在庫数量は 0 以上の整数である必要があります")]
    public int Warehouse89 { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "倉庫在庫数量は 0 以上の整数である必要があります")]
    public int Warehouse81 { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "倉庫在庫数量は 0 以上の整数である必要があります")]
    public int WarehouseInspection { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "倉庫在庫数量は 0 以上の整数である必要があります")]
    public int Warehouse4th { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "未引当数量は 0 以上の整数である必要があります")]
    public int UnallocatedQty { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "出荷数は 0 以上の整数である必要があります")]
    public int ShippedQty { get; set; }
}