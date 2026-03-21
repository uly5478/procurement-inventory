using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 採購訂單 Repository 介面
/// </summary>
public interface IPurchaseOrderRepository
{
    /// <summary>
    /// 查詢採購訂單清單，支援日期區間、廠商名稱、狀態篩選，依 OrderDate 降序排列
    /// </summary>
    Task<IEnumerable<PurchaseOrder>> GetAllAsync(PurchaseOrderQueryDto query);

    /// <summary>
    /// 依 Id 取得採購訂單（含 Items 與 Supplier）
    /// </summary>
    Task<PurchaseOrder?> GetByIdAsync(int id);

    /// <summary>
    /// 建立採購訂單
    /// </summary>
    Task<PurchaseOrder> CreateAsync(PurchaseOrder order);

    /// <summary>
    /// 查詢當日最大流水號，用於產生訂單編號。
    /// 查詢 OrderNumber LIKE 'PO-YYYYMMDD-%' 的最大流水號，若無則回傳 0。
    /// </summary>
    Task<int> GetTodayMaxSequenceAsync(string datePrefix);
}
