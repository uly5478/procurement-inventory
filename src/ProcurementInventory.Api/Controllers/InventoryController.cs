using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Models;
using ProcurementInventory.Api.Services;

namespace ProcurementInventory.Api.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;
    private readonly IExcelExportService _excelService;

    public InventoryController(IInventoryService service, IExcelExportService excelService)
    {
        _service = service;
        _excelService = excelService;
    }

    /// <summary>
    /// 入庫作業（需求 5.1, 5.2）
    /// </summary>
    [HttpPost("stock-in")]
    public async Task<ActionResult<ApiResponse<StockTransactionResultDto>>> StockIn(
        [FromBody] StockInDto dto)
    {
        var operatorAccount = User.Identity?.Name ?? "system";
        var result = await _service.StockInAsync(dto, operatorAccount);
        return Ok(ApiResponse<StockTransactionResultDto>.Ok(result));
    }

    /// <summary>
    /// 出貨作業（需求 6.1, 6.2, 6.3, 6.4）
    /// 超庫存時回傳 requireConfirmation=true，前端確認後帶 forceConfirm=true 重送
    /// </summary>
    [HttpPost("stock-out")]
    public async Task<ActionResult<ApiResponse<StockTransactionResultDto>>> StockOut(
        [FromBody] StockOutDto dto)
    {
        var operatorAccount = User.Identity?.Name ?? "system";
        var result = await _service.StockOutAsync(dto, operatorAccount);
        return Ok(ApiResponse<StockTransactionResultDto>.Ok(result));
    }

    /// <summary>
    /// 查詢指定產品的庫存異動歷程（需求 5.4, 6.5, 8.2）
    /// </summary>
    [HttpGet("{productId:int}/history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StockTransactionDto>>>> GetHistory(
        int productId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var history = await _service.GetTransactionHistoryAsync(productId, startDate, endDate);
        return Ok(ApiResponse<IEnumerable<StockTransactionDto>>.Ok(history));
    }

    /// <summary>
    /// 查詢指定產品的每月出貨統計
    /// </summary>
    [HttpGet("{productId:int}/monthly-summary")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MonthlyShipmentDto>>>> GetMonthlySummary(
        int productId,
        [FromQuery] int months = 6)
    {
        var summary = await _service.GetMonthlyShipmentSummaryAsync(productId, months);
        return Ok(ApiResponse<IEnumerable<MonthlyShipmentDto>>.Ok(summary));
    }

    /// <summary>
    /// 取得所有產品庫存總覽（需求 7.1, 7.2, 7.3）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryOverviewDto>>>> GetOverview()
    {
        var overview = await _service.GetInventoryOverviewAsync();
        return Ok(ApiResponse<IEnumerable<InventoryOverviewDto>>.Ok(overview));
    }

    /// <summary>
    /// 匯出庫存總覽 Excel（需求 7.5, 9.1, 9.2）
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportExcel()
    {
        var overview = await _service.GetInventoryOverviewExtendedAsync();
        var bytes = _excelService.ExportInventoryOverviewExtended(overview);
        var fileName = $"inventory_export_{DateTime.Now:yyyy-MM-dd}.xlsx";
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
