namespace ProcurementInventory.Api.DTOs;

/// <summary>新增產品 DTO</summary>
public class CreateProductDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
}

/// <summary>更新產品 DTO</summary>
public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
}

/// <summary>產品回應 DTO</summary>
public class ProductDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
