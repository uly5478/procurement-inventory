using ProcurementInventory.Api.DTOs;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 採購訂單 Service 介面
/// </summary>
public interface IPurchaseOrderService
{
    /// <summary>查詢採購訂單清單（支援篩選）</summary>
    Task<IEnumerable<PurchaseOrderDto>> GetOrdersAsync(PurchaseOrderQueryDto query);

    /// <summary>依 Id 取得採購訂單</summary>
    Task<PurchaseOrderDto?> GetOrderByIdAsync(int id);

    /// <summary>建立採購訂單</summary>
    Task<PurchaseOrderDto> CreateOrderAsync(CreatePurchaseOrderDto dto, string createdBy);
}
