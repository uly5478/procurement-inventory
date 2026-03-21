using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public PurchaseOrdersController(IPurchaseOrderService service)
    {
        _service = service;
    }

    /// <summary>
    /// 查詢採購訂單清單（需求 4.7）
    /// 支援日期區間、廠商名稱、狀態篩選
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PurchaseOrderDto>>>> GetOrders(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? supplierName,
        [FromQuery] string? status)
    {
        var query = new PurchaseOrderQueryDto
        {
            StartDate = startDate,
            EndDate = endDate,
            SupplierName = supplierName,
            Status = status
        };
        var orders = await _service.GetOrdersAsync(query);
        return Ok(ApiResponse<IEnumerable<PurchaseOrderDto>>.Ok(orders));
    }

    /// <summary>
    /// 取得單一採購訂單（需求 4.1）
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> GetOrder(int id)
    {
        var order = await _service.GetOrderByIdAsync(id);
        if (order is null)
            return NotFound(ApiResponse<PurchaseOrderDto>.Fail("找不到採購訂單"));
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(order));
    }

    /// <summary>
    /// 建立採購訂單（需求 4.1, 4.2, 4.3, 4.4, 4.5, 4.6）
    /// 自動產生訂單編號 PO-YYYYMMDD-NNNN
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> CreateOrder(
        [FromBody] CreatePurchaseOrderDto dto)
    {
        var createdBy = User.Identity?.Name ?? "system";
        var order = await _service.CreateOrderAsync(dto, createdBy);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id },
            ApiResponse<PurchaseOrderDto>.Ok(order));
    }
}
