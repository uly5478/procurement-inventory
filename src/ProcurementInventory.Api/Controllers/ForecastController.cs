using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcurementInventory.Api.Data;
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
    private readonly AppDbContext _db;

    public ForecastController(IDemandForecastService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<DemandForecastDto>>>> GetAllForecasts()
    {
        var forecasts = await _service.GetAllForecastsAsync();
        return Ok(ApiResponse<IEnumerable<DemandForecastDto>>.Ok(forecasts));
    }

    [HttpGet("{productId:int}")]
    public async Task<ActionResult<ApiResponse<ProductForecastDetailDto>>> GetProductForecast(int productId)
    {
        var detail = await _service.GetProductForecastAsync(productId);
        return Ok(ApiResponse<ProductForecastDetailDto>.Ok(detail));
    }

    /// <summary>
    /// 指定月の出荷トランザクション詳細を取得（平均超過月クリック用）
    /// </summary>
    [HttpGet("{productId:int}/monthly-detail/{year:int}/{month:int}")]
    public async Task<ActionResult<ApiResponse<MonthlyShipmentDetailDto>>> GetMonthlyDetail(
        int productId, int year, int month)
    {
        // その月の出荷トランザクションを取得
        var transactions = await _db.StockTransactions
            .Where(t => t.ProductId == productId
                     && t.TransactionType == "出荷"
                     && t.TransactionDate.Year == year
                     && t.TransactionDate.Month == month)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync();

        // 全月の平均を計算
        var allMonthly = await _db.MonthlyShipments
            .Where(m => m.ProductId == productId)
            .ToListAsync();

        decimal average = allMonthly.Count > 0
            ? (decimal)allMonthly.Average(m => m.Quantity)
            : 0;

        // MonthlyShipments から当月合計を取得
        var monthlyRecord = await _db.MonthlyShipments
            .FirstOrDefaultAsync(m => m.ProductId == productId && m.Year == year && m.Month == month);

        int totalShipped = monthlyRecord?.Quantity
            ?? transactions.Sum(t => t.Quantity);

        var result = new MonthlyShipmentDetailDto
        {
            Year = year,
            Month = month,
            TotalShipped = totalShipped,
            Average = Math.Round(average, 1),
            AboveAverage = totalShipped > average,
            Transactions = transactions.Select(t => new ShipmentTransactionDto
            {
                Id = t.Id,
                TransactionDate = t.TransactionDate,
                Quantity = t.Quantity,
                OperatorAccount = t.OperatorAccount,
                Remark = t.Remark,
            }).ToList()
        };

        return Ok(ApiResponse<MonthlyShipmentDetailDto>.Ok(result));
    }
}
