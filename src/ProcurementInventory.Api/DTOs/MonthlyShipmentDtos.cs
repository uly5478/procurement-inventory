using System.ComponentModel.DataAnnotations;

namespace ProcurementInventory.Api.DTOs;

/// <summary>月別出荷実績 DTO</summary>
public class MonthlyShipmentRecordDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int Quantity { get; set; }
}

/// <summary>月別出荷実績登録・更新 DTO</summary>
public class UpsertMonthlyShipmentDto
{
    public int Year { get; set; }

    [Range(1, 12, ErrorMessage = "月は 1〜12 の範囲で入力してください")]
    public int Month { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "出荷数量は 0 以上の整数を入力してください")]
    public int Quantity { get; set; }
}

/// <summary>月別出荷実績一括登録 DTO</summary>
public class BulkUpsertMonthlyShipmentsDto
{
    public int Year { get; set; }

    /// <summary>Key: Month (1-12), Value: Quantity</summary>
    public Dictionary<int, int> MonthQuantities { get; set; } = new();
}

/// <summary>月別出荷実績結果（12ヶ月分）DTO</summary>
public class MonthlyShipmentResultDto
{
    public int Year { get; set; }
    public int Jan { get; set; }
    public int Feb { get; set; }
    public int Mar { get; set; }
    public int Apr { get; set; }
    public int May { get; set; }
    public int Jun { get; set; }
    public int Jul { get; set; }
    public int Aug { get; set; }
    public int Sep { get; set; }
    public int Oct { get; set; }
    public int Nov { get; set; }
    public int Dec { get; set; }
}