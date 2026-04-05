using System.ComponentModel.DataAnnotations;

namespace ProcurementInventory.Api.DTOs;

/// <summary>新增產品 DTO</summary>
public class CreateProductDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "箱入数は 1 以上の整数を入力してください")]
    public int BoxQty { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "MOQ は 1 以上の整数を入力してください")]
    public int MOQ { get; set; } = 1;

    [Range(0, int.MaxValue, ErrorMessage = "安全在庫は 0 以上の整数を入力してください")]
    public int SafetyStock { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "平均出荷数は 0 以上の数値を入力してください")]
    public decimal AverageShipment { get; set; } = 0;

    /// <summary>仕入分類コード</summary>
    public string? CategoryCode { get; set; }
}

/// <summary>更新產品 DTO</summary>
public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "箱入数は 1 以上の整数を入力してください")]
    public int BoxQty { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "MOQ は 1 以上の整数を入力してください")]
    public int MOQ { get; set; } = 1;

    [Range(0, int.MaxValue, ErrorMessage = "安全在庫は 0 以上の整数を入力してください")]
    public int SafetyStock { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "平均出荷数は 0 以上の数値を入力してください")]
    public decimal AverageShipment { get; set; } = 0;

    /// <summary>仕入分類コード</summary>
    public string? CategoryCode { get; set; }
}

/// <summary>產品回應 DTO</summary>
public class ProductDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int BoxQty { get; set; }
    public int MOQ { get; set; }
    public int SafetyStock { get; set; }
    public decimal AverageShipment { get; set; }
    public string? CategoryCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
