using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Models;
using ProcurementInventory.Api.Services;

namespace ProcurementInventory.Api.Controllers;

/// <summary>
/// 倉庫別在庫 API
/// </summary>
[ApiController]
[Route("api/products/{productId}/warehouse-stock")]
[Authorize]
public class WarehouseStockController : ControllerBase
{
    private readonly IWarehouseStockService _service;

    public WarehouseStockController(IWarehouseStockService service)
    {
        _service = service;
    }

    /// <summary>倉庫別在庫を取得</summary>
    [HttpGet]
    public async Task<ApiResponse<WarehouseStockDto>> Get([FromRoute] int productId)
    {
        var result = await _service.GetWarehouseStockAsync(productId);
        return ApiResponse<WarehouseStockDto>.Ok(result);
    }

    /// <summary>倉庫別在庫を更新</summary>
    [HttpPut]
    public async Task<ApiResponse<WarehouseStockDto>> Update(
        [FromRoute] int productId, 
        [FromBody] UpdateWarehouseStockDto dto)
    {
        var result = await _service.UpdateWarehouseStockAsync(productId, dto);
        return ApiResponse<WarehouseStockDto>.Ok(result);
    }
}