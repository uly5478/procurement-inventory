using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 廠商報價 Repository 介面
/// </summary>
public interface ISupplierRepository
{
    /// <summary>
    /// 取得指定產品的最新報價（IsCurrent=true），依 UnitPrice 升序排列。
    /// </summary>
    Task<IEnumerable<ProductSupplierPrice>> GetCurrentPricesByProductIdAsync(int productId);

    /// <summary>
    /// 取得指定產品的所有歷史報價記錄。
    /// </summary>
    Task<IEnumerable<ProductSupplierPrice>> GetAllPricesByProductIdAsync(int productId);

    /// <summary>
    /// 計算指定產品目前有幾家不重複廠商（IsCurrent=true 的不重複 SupplierId 數）。
    /// </summary>
    Task<int> GetCurrentSupplierCountAsync(int productId);

    /// <summary>新增報價記錄</summary>
    Task<ProductSupplierPrice> CreatePriceAsync(ProductSupplierPrice price);

    /// <summary>依 Id 取得報價記錄</summary>
    Task<ProductSupplierPrice?> GetPriceByIdAsync(int id);

    /// <summary>更新報價記錄</summary>
    Task<ProductSupplierPrice> UpdatePriceAsync(ProductSupplierPrice price);

    /// <summary>依廠商名稱查詢廠商（找不到則回傳 null）</summary>
    Task<Supplier?> GetSupplierByNameAsync(string name);

    /// <summary>新增廠商</summary>
    Task<Supplier> CreateSupplierAsync(Supplier supplier);
}
