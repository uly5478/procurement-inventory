using System.ComponentModel.DataAnnotations;

namespace ProcurementInventory.Api.DTOs;

/// <summary>月別在庫スナップショット DTO</summary>
public class MonthlyInventoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int OrderQty { get; set; }
    public int StockQty { get; set; }
    public decimal StockAmount { get; set; }
    public decimal TurnoverRate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>月別在庫スナップショット記録 DTO</summary>
public class RecordMonthlyInventoryDto
{
    [Range(0, int.MaxValue)]
    public int OrderQty { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQty { get; set; }

    [Range(0, double.MaxValue)]
    public decimal StockAmount { get; set; }

    /// <summary>回転率計算用の月別出荷金額</summary>
    public decimal MonthlyShipmentAmount { get; set; }
}