using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Models;
using ProcurementInventory.Api.Services;

namespace ProcurementInventory.Api.Controllers;

[ApiController]
[Route("api/forecast")]
[Authorize]
public class ForecastController : ControllerBase
{
    private readonly IDemandForecastService _service;

    public ForecastController(IDemandForecastService service)
    {
        _service = service;
    }

    /// <summary>
    /// 取得所有產品的需求預測結果（需求 9.1, 9.2, 9.3）
    /// 資料不足的產品不會出現在清單中
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<DemandForecastDto>>>> GetAllForecasts()
    {
        var forecasts = await _service.GetAllForecastsAsync();
        return Ok(ApiResponse<IEnumerable<DemandForecastDto>>.Ok(forecasts));
    }

    /// <summary>
    /// 取得單一產品的需求預測詳情（需求 9.2, 9.4, 9.7）
    /// 含歷史出貨量與信賴區間，資料不足時回傳 errorMessage
    /// </summary>
    [HttpGet("{productId:int}")]
    public async Task<ActionResult<ApiResponse<ProductForecastDetailDto>>> GetProductForecast(
        int productId)
    {
        var detail = await _service.GetProductForecastAsync(productId);
        return Ok(ApiResponse<ProductForecastDetailDto>.Ok(detail));
    }
}
