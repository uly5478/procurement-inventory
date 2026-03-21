using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Models;
using ProcurementInventory.Api.Services;

namespace ProcurementInventory.Api.Controllers;

[ApiController]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>
    /// 取得指定產品的廠商報價清單（依買價升序）
    /// 需求 2.5
    /// </summary>
    [HttpGet("api/products/{productId:int}/suppliers")]
    public async Task<ActionResult<ApiResponse<SupplierPriceListResult>>> GetProductSuppliers(int productId)
    {
        var result = await _supplierService.GetProductSuppliersAsync(productId);
        return Ok(ApiResponse<SupplierPriceListResult>.Ok(result));
    }

    /// <summary>
    /// 新增廠商報價。
    /// 若已有 4 家廠商且 ForceCreate=false，回傳 200 + RequireConfirmation=true。
    /// 成功新增時回傳 201 Created。
    /// 需求 2.1, 2.2, 2.4
    /// </summary>
    [HttpPost("api/products/{productId:int}/suppliers")]
    public async Task<ActionResult<ApiResponse<SupplierPriceListResult>>> AddSupplierPrice(
        int productId,
        [FromBody] CreateSupplierPriceDto dto)
    {
        var result = await _supplierService.AddSupplierPriceAsync(productId, dto);

        if (result.RequireConfirmation)
        {
            // 需求 2.4：第 5 家廠商警告，回傳 200 + warning
            return Ok(ApiResponse<SupplierPriceListResult>.Ok(result));
        }

        // 成功新增，回傳 201 Created
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SupplierPriceListResult>.Ok(result));
    }

    /// <summary>
    /// 更新廠商報價（保留歷史記錄）
    /// 需求 2.3
    /// </summary>
    [HttpPut("api/suppliers/{priceId:int}")]
    public async Task<ActionResult<ApiResponse<SupplierPriceDto>>> UpdateSupplierPrice(
        int priceId,
        [FromBody] UpdateSupplierPriceDto dto)
    {
        var result = await _supplierService.UpdateSupplierPriceAsync(priceId, dto);
        return Ok(ApiResponse<SupplierPriceDto>.Ok(result));
    }
}
