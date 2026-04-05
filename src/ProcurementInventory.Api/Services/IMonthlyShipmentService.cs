using ProcurementInventory.Api.DTOs;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 月別出荷実績 Service 介面
/// </summary>
public interface IMonthlyShipmentService
{
    /// <summary>商品IDで月別出荷実績を取得</summary>
    Task<MonthlyShipmentResultDto?> GetMonthlyShipmentsAsync(int productId, int year);

    /// <summary>単一レコードを登録または更新</summary>
    Task<MonthlyShipmentRecordDto> UpsertMonthlyShipmentAsync(int productId, UpsertMonthlyShipmentDto dto);

    /// <summary>一括登録・更新（12ヶ月分）</summary>
    Task<IEnumerable<MonthlyShipmentRecordDto>> BulkUpsertMonthlyShipmentsAsync(int productId, BulkUpsertMonthlyShipmentsDto dto);
}