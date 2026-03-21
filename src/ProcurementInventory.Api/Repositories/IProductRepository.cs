using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 產品 Repository 介面
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// 查詢產品清單，支援關鍵字模糊搜尋與 isActive 篩選。
    /// isActive=true（預設）只回傳啟用產品；isActive=false 只回傳停用；isActive=null 回傳全部。
    /// </summary>
    Task<IEnumerable<Product>> GetAllAsync(string? keyword, bool? isActive);

    /// <summary>依 Id 取得產品</summary>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>依產品編號取得產品</summary>
    Task<Product?> GetByCodeAsync(string productCode);

    /// <summary>新增產品</summary>
    Task<Product> CreateAsync(Product product);

    /// <summary>更新產品</summary>
    Task<Product> UpdateAsync(Product product);

    /// <summary>
    /// 檢查產品編號是否已存在。
    /// excludeId 用於更新時排除自身。
    /// </summary>
    Task<bool> ExistsAsync(string productCode, int? excludeId = null);
}
