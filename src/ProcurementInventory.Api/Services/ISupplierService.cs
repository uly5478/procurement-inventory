using ProcurementInventory.Api.DTOs;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 廠商報價 Service 介面
/// </summary>
public interface ISupplierService
{
    /// <summary>
    /// 取得指定產品的最新廠商報價清單（依買價升序）。
    /// </summary>
    Task<SupplierPriceListResult> GetProductSuppliersAsync(int productId);

    /// <summary>
    /// 新增廠商報價。
    /// 若已有 4 家廠商且 ForceCreate=false，回傳含警告的結果（不直接新增）。
    /// 若 ForceCreate=true，直接新增。
    /// </summary>
    Task<SupplierPriceListResult> AddSupplierPriceAsync(int productId, CreateSupplierPriceDto dto);

    /// <summary>
    /// 更新廠商報價：將舊報價 IsCurrent 設為 false，建立新報價記錄（IsCurrent=true）。
    /// </summary>
    Task<SupplierPriceDto> UpdateSupplierPriceAsync(int priceId, UpdateSupplierPriceDto dto);
}
