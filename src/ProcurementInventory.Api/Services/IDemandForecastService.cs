using ProcurementInventory.Api.DTOs;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 需求預測 Service 介面
/// </summary>
public interface IDemandForecastService
{
    /// <summary>
    /// 取得所有產品的需求預測結果（下個月）
    /// </summary>
    Task<IEnumerable<DemandForecastDto>> GetAllForecastsAsync();

    /// <summary>
    /// 取得單一產品的需求預測詳情（含歷史出貨量）
    /// 資料不足（< 3 個月）時回傳 ErrorMessage
    /// </summary>
    Task<ProductForecastDetailDto> GetProductForecastAsync(int productId);
}
