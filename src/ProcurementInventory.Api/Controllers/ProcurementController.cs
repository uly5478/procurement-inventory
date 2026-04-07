using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Models;
using ProcurementInventory.Api.Services;

namespace ProcurementInventory.Api.Controllers;

[ApiController]
[Route("api/procurement")]
[Authorize]
public class ProcurementController : ControllerBase
{
    private readonly IProcurementCalculatorService _procurementService;

    public ProcurementController(IProcurementCalculatorService procurementService)
    {
        _procurementService = procurementService;
    }

    /// <summary>
    /// 取得所有產品的採購建議清單（需求 3.1, 3.2, 3.3, 3.4, 3.5）
    /// </summary>
    /// <param name="useForecast">是否使用需求預測值（需求 9.6）</param>
    [HttpGet("suggestions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProcurementSuggestionDto>>>> GetSuggestions(
        [FromQuery] bool useForecast = false)
    {
        var suggestions = await _procurementService.GetAllSuggestionsAsync(useForecast);
        return Ok(ApiResponse<IEnumerable<ProcurementSuggestionDto>>.Ok(suggestions));
    }

    /// <summary>
    /// 手動覆寫指定產品的採購量（需求 3.6）
    /// </summary>
    /// <param name="productId">產品 ID</param>
    /// <param name="dto">手動覆寫資料</param>
    [HttpPut("suggestions/{productId:int}")]
    public async Task<ActionResult<ApiResponse<ProcurementSuggestionDto>>> ManualOverride(
        int productId,
        [FromBody] ManualOverrideDto dto)
    {
        var suggestion = await _procurementService.ManualOverrideAsync(productId, dto.ManualOverrideQty);
        return Ok(ApiResponse<ProcurementSuggestionDto>.Ok(suggestion));
    }

    /// <summary>手動覆寫をリセットしてシステム計算値に戻す</summary>
    [HttpDelete("suggestions/{productId:int}/override")]
    public async Task<ActionResult<ApiResponse<ProcurementSuggestionDto>>> ResetOverride(int productId)
    {
        var suggestion = await _procurementService.ResetOverrideAsync(productId);
        return Ok(ApiResponse<ProcurementSuggestionDto>.Ok(suggestion));
    }

    /// <summary>
    /// 取得採購設定（需求 3.2, 3.3）
    /// </summary>
    [HttpGet("settings")]
    public async Task<ActionResult<ApiResponse<ProcurementSettingsDto>>> GetSettings()
    {
        var settings = await _procurementService.GetSettingsAsync();
        return Ok(ApiResponse<ProcurementSettingsDto>.Ok(settings));
    }

    /// <summary>
    /// 更新採購設定（需求 3.3）
    /// </summary>
    /// <param name="dto">更新採購設定資料（DefaultTurnoverMonths 範圍 1.0–6.0）</param>
    [HttpPut("settings")]
    public async Task<ActionResult<ApiResponse<ProcurementSettingsDto>>> UpdateSettings(
        [FromBody] UpdateProcurementSettingsDto dto)
    {
        var settings = await _procurementService.UpdateSettingsAsync(dto);
        return Ok(ApiResponse<ProcurementSettingsDto>.Ok(settings));
    }
}
