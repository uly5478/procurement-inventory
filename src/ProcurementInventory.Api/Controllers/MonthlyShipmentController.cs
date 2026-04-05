using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Models;
using ProcurementInventory.Api.Services;

namespace ProcurementInventory.Api.Controllers;

/// <summary>
/// 月別出荷実績 API
/// </summary>
[ApiController]
[Route("api/products/{productId}/monthly-shipments")]
[Authorize]
public class MonthlyShipmentController : ControllerBase
{
    private readonly IMonthlyShipmentService _service;

    public MonthlyShipmentController(IMonthlyShipmentService service)
    {
        _service = service;
    }

    /// <summary>月別出荷実績を取得</summary>
    [HttpGet]
    public async Task<ApiResponse<MonthlyShipmentResultDto>> Get([FromRoute] int productId, [FromQuery] int? year = null)
    {
        var currentYear = year ?? DateTime.UtcNow.Year;
        var result = await _service.GetMonthlyShipmentsAsync(productId, currentYear);
        return ApiResponse<MonthlyShipmentResultDto>.Ok(result!);
    }

    /// <summary>単一レコードを登録または更新</summary>
    [HttpPost]
    public async Task<ApiResponse<MonthlyShipmentRecordDto>> Upsert([FromRoute] int productId, [FromBody] UpsertMonthlyShipmentDto dto)
    {
        var result = await _service.UpsertMonthlyShipmentAsync(productId, dto);
        return ApiResponse<MonthlyShipmentRecordDto>.Ok(result);
    }

    /// <summary>一括登録・更新（12ヶ月分）</summary>
    [HttpPut("bulk")]
    public async Task<ApiResponse<IEnumerable<MonthlyShipmentRecordDto>>> BulkUpsert(
        [FromRoute] int productId, 
        [FromBody] BulkUpsertMonthlyShipmentsDto dto)
    {
        var result = await _service.BulkUpsertMonthlyShipmentsAsync(productId, dto);
        return ApiResponse<IEnumerable<MonthlyShipmentRecordDto>>.Ok(result);
    }
}