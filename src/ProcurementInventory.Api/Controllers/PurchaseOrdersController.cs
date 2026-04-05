using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ProcurementInventory.Api.Data;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Models;
using ProcurementInventory.Api.Services;

namespace ProcurementInventory.Api.Controllers;

[ApiController]
[Route("api/purchase-orders")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _service;
    private readonly AppDbContext _db;

    public PurchaseOrdersController(IPurchaseOrderService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PurchaseOrderDto>>>> GetOrders(
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
        [FromQuery] string? supplierName, [FromQuery] string? status)
    {
        var query = new PurchaseOrderQueryDto { StartDate = startDate, EndDate = endDate, SupplierName = supplierName, Status = status };
        var orders = await _service.GetOrdersAsync(query);
        return Ok(ApiResponse<IEnumerable<PurchaseOrderDto>>.Ok(orders));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> GetOrder(int id)
    {
        var order = await _service.GetOrderByIdAsync(id);
        if (order is null) return NotFound(ApiResponse<PurchaseOrderDto>.Fail("発注が見つかりません"));
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(order));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> CreateOrder([FromBody] CreatePurchaseOrderDto dto)
    {
        var createdBy = User.Identity?.Name ?? "system";
        var order = await _service.CreateOrderAsync(dto, createdBy);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, ApiResponse<PurchaseOrderDto>.Ok(order));
    }

    /// <summary>ステータス変更</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _db.PurchaseOrders.FindAsync(id);
        if (order is null) return NotFound(ApiResponse<PurchaseOrderDto>.Fail("発注が見つかりません"));

        var validStatuses = new[] { "待確認", "已確認", "已入荷", "キャンセル" };
        if (!validStatuses.Contains(dto.Status))
            return BadRequest(ApiResponse<PurchaseOrderDto>.Fail("無効なステータスです"));

        order.Status = dto.Status;
        await _db.SaveChangesAsync();

        var result = await _service.GetOrderByIdAsync(id);
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result!));
    }

    /// <summary>発注統計</summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderStatsDto>>> GetStats()
    {
        var orders = await _db.PurchaseOrders.Include(o => o.Supplier).ToListAsync();

        var monthly = orders
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyOrderStatDto
            {
                Label = $"{g.Key.Year}/{g.Key.Month:D2}",
                OrderCount = g.Count(),
                TotalAmount = g.Sum(o => o.TotalAmount)
            }).ToList();

        var bySupplier = orders
            .GroupBy(o => o.Supplier?.Name ?? "不明")
            .Select(g => new SupplierOrderStatDto
            {
                SupplierName = g.Key,
                OrderCount = g.Count(),
                TotalAmount = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(s => s.TotalAmount).ToList();

        var stats = new PurchaseOrderStatsDto
        {
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(o => o.Status == "待確認"),
            ConfirmedOrders = orders.Count(o => o.Status == "已確認"),
            ReceivedOrders = orders.Count(o => o.Status == "已入荷"),
            CancelledOrders = orders.Count(o => o.Status == "キャンセル"),
            TotalAmount = orders.Sum(o => o.TotalAmount),
            MonthlyStats = monthly,
            SupplierStats = bySupplier
        };

        return Ok(ApiResponse<PurchaseOrderStatsDto>.Ok(stats));
    }

    /// <summary>仕入先別発注書 Excel 出力（全発注を仕入先ごとにシート分け）</summary>
    [HttpGet("export-by-supplier")]
    public async Task<IActionResult> ExportBySupplier([FromQuery] string? supplierName)
    {
        var query = _db.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .AsQueryable();

        if (!string.IsNullOrEmpty(supplierName))
            query = query.Where(o => o.Supplier != null && o.Supplier.Name == supplierName);

        var orders = await query.OrderBy(o => o.Supplier!.Name).ThenByDescending(o => o.OrderDate).ToListAsync();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();

        // 仕入先ごとにシートを作成
        var grouped = orders.GroupBy(o => o.Supplier?.Name ?? "不明");
        foreach (var group in grouped)
        {
            var ws = package.Workbook.Worksheets.Add(group.Key);
            int row = 1;

            // 仕入先ヘッダー
            ws.Cells[row, 1].Value = $"仕入先：{group.Key}";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 14;
            row += 2;

            foreach (var order in group)
            {
                // 発注情報
                ws.Cells[row, 1].Value = $"発注番号：{order.OrderNumber}";
                ws.Cells[row, 3].Value = $"発注日：{order.OrderDate:yyyy-MM-dd}";
                ws.Cells[row, 1].Style.Font.Bold = true;
                row++;

                // 明細ヘッダー
                var headers = new[] { "商品コード", "商品名", "数量", "単価", "小計" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[row, i + 1].Value = headers[i];
                    ws.Cells[row, i + 1].Style.Font.Bold = true;
                    ws.Cells[row, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue);
                }
                row++;

                // 明細行
                foreach (var item in order.Items)
                {
                    ws.Cells[row, 1].Value = item.Product?.ProductCode;
                    ws.Cells[row, 2].Value = item.Product?.Name;
                    ws.Cells[row, 3].Value = item.Quantity;
                    ws.Cells[row, 4].Value = (double)item.UnitPrice;
                    ws.Cells[row, 4].Style.Numberformat.Format = "0.0000";
                    ws.Cells[row, 5].Value = (double)item.Subtotal;
                    ws.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                    row++;
                }

                // 小計行
                ws.Cells[row, 4].Value = "合計";
                ws.Cells[row, 4].Style.Font.Bold = true;
                ws.Cells[row, 5].Value = (double)order.TotalAmount;
                ws.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 5].Style.Font.Bold = true;
                row += 2; // 発注間の空行
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();
        }

        if (!package.Workbook.Worksheets.Any())
            package.Workbook.Worksheets.Add("発注書（データなし）");

        var bytes = package.GetAsByteArray();
        var fileName = string.IsNullOrEmpty(supplierName)
            ? $"発注書_全仕入先_{DateTime.Now:yyyyMMdd}.xlsx"
            : $"発注書_{supplierName}_{DateTime.Now:yyyyMMdd}.xlsx";

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
    public async Task<IActionResult> ExportOrder(int id)
    {
        var order = await _db.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("発注書");

        // ヘッダー情報
        ws.Cells[1, 1].Value = "発注書";
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.Font.Size = 16;
        ws.Cells[2, 1].Value = $"発注番号：{order.OrderNumber}";
        ws.Cells[3, 1].Value = $"仕入先：{order.Supplier?.Name}";
        ws.Cells[4, 1].Value = $"発注日：{order.OrderDate:yyyy-MM-dd}";
        ws.Cells[5, 1].Value = $"ステータス：{order.Status}";
        ws.Cells[6, 1].Value = $"作成者：{order.CreatedBy}";

        // 明細ヘッダー
        int row = 8;
        var headers = new[] { "商品名", "数量", "単価", "小計" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[row, i + 1].Value = headers[i];
            ws.Cells[row, i + 1].Style.Font.Bold = true;
            ws.Cells[row, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue);
        }

        // 明細
        row = 9;
        foreach (var item in order.Items)
        {
            ws.Cells[row, 1].Value = item.Product?.Name;
            ws.Cells[row, 2].Value = item.Quantity;
            ws.Cells[row, 3].Value = (double)item.UnitPrice;
            ws.Cells[row, 3].Style.Numberformat.Format = "0.0000";
            ws.Cells[row, 4].Value = (double)item.Subtotal;
            ws.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
            row++;
        }

        // 合計
        ws.Cells[row, 3].Value = "合計";
        ws.Cells[row, 3].Style.Font.Bold = true;
        ws.Cells[row, 4].Value = (double)order.TotalAmount;
        ws.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
        ws.Cells[row, 4].Style.Font.Bold = true;

        ws.Cells[ws.Dimension.Address].AutoFitColumns();

        var bytes = package.GetAsByteArray();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"発注書_{order.OrderNumber}.xlsx");
    }
}

/// <summary>ステータス更新 DTO</summary>
public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
}
