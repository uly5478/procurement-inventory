using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Models;
using ProcurementInventory.Api.Services;

namespace ProcurementInventory.Api.Controllers;

/// <summary>
/// 月別在庫スナップショット API
/// </summary>
[ApiController]
[Route("api/products/{productId}/monthly-inventory")]
[Authorize]
public class MonthlyInventoryController : ControllerBase
{
    private readonly IMonthlyInventoryService _service;

    public MonthlyInventoryController(IMonthlyInventoryService service)
    {
        _service = service;
    }

    /// <summary>月別在庫を取得</summary>
    [HttpGet]
    public async Task<ApiResponse<IEnumerable<MonthlyInventoryDto>>> Get(
        [FromRoute] int productId, 
        [FromQuery] int months = 12)
    {
        var result = await _service.GetMonthlyInventoryAsync(productId, months);
        return ApiResponse<IEnumerable<MonthlyInventoryDto>>.Ok(result);
    }

    /// <summary>月別在庫スナップショットを記録</summary>
    [HttpPost]
    public async Task<ApiResponse<MonthlyInventoryDto>> Record(
        [FromRoute] int productId,
        [FromBody] RecordMonthlyInventoryDto dto)
    {
        var now = DateTime.UtcNow;
        var result = await _service.RecordMonthlySnapshotAsync(productId, now.Year, now.Month, dto);
        return ApiResponse<MonthlyInventoryDto>.Ok(result);
    }
}