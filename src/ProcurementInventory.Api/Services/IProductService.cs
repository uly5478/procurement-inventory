using ProcurementInventory.Api.DTOs;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 產品 Service 介面
/// </summary>
public interface IProductService
{
    /// <summary>查詢產品清單（支援關鍵字搜尋、狀態篩選、仕入分類コードフィルター）</summary>
    Task<IEnumerable<ProductDto>> GetProductsAsync(string? keyword, bool? isActive, string? categoryCode = null);

    /// <summary>依 Id 取得產品</summary>
    Task<ProductDto?> GetProductByIdAsync(int id);

    /// <summary>新增產品（若產品編號重複則拋出 ArgumentException）</summary>
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);

    /// <summary>更新產品（自動記錄 UpdatedAt）</summary>
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto);

    /// <summary>停用產品（設定 IsActive = false）</summary>
    Task DeactivateProductAsync(int id);
}
