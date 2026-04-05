using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Entities;
using ProcurementInventory.Api.Repositories;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 庫存管理 Service 實作
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repo;
    private readonly IProcurementRepository _procurementRepo;
    private readonly IWarehouseStockRepository _warehouseStockRepo;
    private readonly IMonthlyShipmentRepository _monthlyShipmentRepo;
    private readonly ISupplierRepository _supplierRepo;

    public InventoryService(
        IInventoryRepository repo, 
        IProcurementRepository procurementRepo,
        IWarehouseStockRepository warehouseStockRepo,
        IMonthlyShipmentRepository monthlyShipmentRepo,
        ISupplierRepository supplierRepo)
    {
        _repo = repo;
        _procurementRepo = procurementRepo;
        _warehouseStockRepo = warehouseStockRepo;
        _monthlyShipmentRepo = monthlyShipmentRepo;
        _supplierRepo = supplierRepo;
    }

    /// <inheritdoc/>
    public async Task<StockTransactionResultDto> StockInAsync(StockInDto dto, string operatorAccount)
    {
        if (dto.Quantity <= 0)
            throw new ArgumentException("入庫數量必須大於 0");

        var record = await _repo.GetByProductIdAsync(dto.ProductId);
        int stockBefore;

        if (record is null)
        {
            // 首次入庫，建立庫存記錄
            stockBefore = 0;
            record = new InventoryRecord
            {
                ProductId = dto.ProductId,
                CurrentStock = dto.Quantity,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.CreateAsync(record);
        }
        else
        {
            stockBefore = record.CurrentStock;
            record.CurrentStock += dto.Quantity;
            record.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(record);
        }

        var transaction = new StockTransaction
        {
            ProductId = dto.ProductId,
            TransactionType = "入庫",
            Quantity = dto.Quantity,
            StockBefore = stockBefore,
            StockAfter = record.CurrentStock,
            PurchaseOrderId = dto.PurchaseOrderId,
            TransactionDate = dto.TransactionDate,
            OperatorAccount = operatorAccount,
            CreatedAt = DateTime.UtcNow,
            Remark = dto.Remark
        };

        var saved = await _repo.AddTransactionAsync(transaction);

        return new StockTransactionResultDto
        {
            TransactionId = saved.Id,
            ProductId = dto.ProductId,
            ProductName = record.Product?.Name ?? string.Empty,
            TransactionType = "入庫",
            Quantity = dto.Quantity,
            StockBefore = stockBefore,
            StockAfter = record.CurrentStock,
            TransactionDate = dto.TransactionDate,
            OperatorAccount = operatorAccount,
            Remark = dto.Remark
        };
    }

    /// <inheritdoc/>
    public async Task<StockTransactionResultDto> StockOutAsync(StockOutDto dto, string operatorAccount)
    {
        if (dto.Quantity <= 0)
            throw new ArgumentException("出貨數量必須大於 0");

        var record = await _repo.GetByProductIdAsync(dto.ProductId);
        int currentStock = record?.CurrentStock ?? 0;

        // 超庫存警告
        if (dto.Quantity > currentStock && !dto.ForceConfirm)
        {
            return new StockTransactionResultDto
            {
                ProductId = dto.ProductId,
                TransactionType = "出貨",
                Quantity = dto.Quantity,
                StockBefore = currentStock,
                StockAfter = currentStock,
                Warning = $"出貨數量（{dto.Quantity}）超過當前庫存（{currentStock}），是否確認執行？",
                RequireConfirmation = true
            };
        }

        int stockBefore = currentStock;
        int stockAfter = currentStock - dto.Quantity;

        if (record is null)
        {
            record = new InventoryRecord
            {
                ProductId = dto.ProductId,
                CurrentStock = stockAfter,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.CreateAsync(record);
        }
        else
        {
            record.CurrentStock = stockAfter;
            record.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(record);
        }

        var transaction = new StockTransaction
        {
            ProductId = dto.ProductId,
            TransactionType = "出貨",
            Quantity = dto.Quantity,
            StockBefore = stockBefore,
            StockAfter = stockAfter,
            TransactionDate = dto.TransactionDate,
            OperatorAccount = operatorAccount,
            CreatedAt = DateTime.UtcNow,
            Remark = dto.Remark
        };

        var saved = await _repo.AddTransactionAsync(transaction);

        return new StockTransactionResultDto
        {
            TransactionId = saved.Id,
            ProductId = dto.ProductId,
            ProductName = record.Product?.Name ?? string.Empty,
            TransactionType = "出貨",
            Quantity = dto.Quantity,
            StockBefore = stockBefore,
            StockAfter = stockAfter,
            TransactionDate = dto.TransactionDate,
            OperatorAccount = operatorAccount,
            Remark = dto.Remark
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<StockTransactionDto>> GetTransactionHistoryAsync(
        int productId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var transactions = await _repo.GetTransactionsByProductAsync(productId, startDate, endDate);
        return transactions.Select(t => new StockTransactionDto
        {
            Id = t.Id,
            ProductId = t.ProductId,
            TransactionType = t.TransactionType,
            Quantity = t.Quantity,
            StockBefore = t.StockBefore,
            StockAfter = t.StockAfter,
            PurchaseOrderId = t.PurchaseOrderId,
            TransactionDate = t.TransactionDate,
            OperatorAccount = t.OperatorAccount,
            CreatedAt = t.CreatedAt,
            Remark = t.Remark
        });
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MonthlyShipmentDto>> GetMonthlyShipmentSummaryAsync(
        int productId, int months = 6)
    {
        var summaries = await _repo.GetMonthlyShipmentSummaryAsync(productId, months);
        return summaries.Select(s => new MonthlyShipmentDto
        {
            Year = s.Year,
            Month = s.Month,
            TotalShipped = s.TotalShipped
        });
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InventoryOverviewDto>> GetInventoryOverviewAsync()
    {
        var records = await _repo.GetAllAsync();
        var result = new List<InventoryOverviewDto>();

        // 取得全域設定（回転月数）
        var settings = await _procurementRepo.GetSettingsAsync();
        decimal turnoverMonths = settings.DefaultTurnoverMonths;

        foreach (var record in records)
        {
            // 平均出荷数（商品マスタの値を優先、なければ過去6ヶ月実績から計算）
            decimal avgShipment = record.Product?.AverageShipment ?? 0;
            if (avgShipment == 0)
            {
                var monthlySummaries = await _repo.GetMonthlyShipmentSummaryAsync(record.ProductId, 6);
                var summaryList = monthlySummaries.ToList();
                avgShipment = summaryList.Count > 0
                    ? (decimal)summaryList.Sum(s => s.TotalShipped) / summaryList.Count
                    : 0;
            }
            // 倉庫在庫
            var warehouseStock = await _warehouseStockRepo.GetByProductIdAsync(record.ProductId);
            int totalWarehouseStock = warehouseStock.Warehouse89 + warehouseStock.Warehouse81
                + warehouseStock.WarehouseInspection + warehouseStock.Warehouse4th;

            // 安全在庫（表示用のみ）
            int safetyStock = record.Product?.SafetyStock ?? 0;

            // リードタイム月数（推奨仕入先から取得）
            var currentPrices = (await _supplierRepo.GetCurrentPricesByProductIdAsync(record.ProductId))
                .OrderBy(p => p.UnitPrice).ToList();
            decimal leadTimeMonths = currentPrices.Count > 0
                ? currentPrices[0].LeadTimeDays / 30m
                : 0m;

            // 庫存狀態：低於安全在庫 → Low
            string status = safetyStock > 0 && totalWarehouseStock < safetyStock ? "Low" : "Normal";

            // 公式: 発注数 = 平均出荷数 × (回転月数 + リードタイム月数) - (倉庫合計 - 未引当数量)
            int effectiveStock = totalWarehouseStock - warehouseStock.UnallocatedQty;

            // 半年分の月次発注提案を計算
            var suggestions = new List<MonthlyOrderSuggestionDto>();
            decimal estimatedStock = effectiveStock;
            var now = DateTime.UtcNow;

            for (int i = 0; i < 6; i++)
            {
                var targetDate = now.AddMonths(i);
                int targetYear = targetDate.Year;
                int targetMonth = targetDate.Month;

                // 今月以降は前月在庫 - 平均出荷数で推移
                if (i > 0)
                    estimatedStock = Math.Max(0, estimatedStock - avgShipment);

                decimal needed = avgShipment * (turnoverMonths + leadTimeMonths) - estimatedStock;
                int suggestedQty = needed > 0 ? (int)Math.Ceiling(needed) : 0;

                // MOQ/BoxQty 丸め
                int moq = record.Product?.MOQ ?? 1;
                int boxQty = record.Product?.BoxQty ?? 1;
                if (suggestedQty > 0)
                {
                    if (suggestedQty < moq) suggestedQty = moq;
                    if (boxQty > 1 && suggestedQty % boxQty != 0)
                        suggestedQty = (suggestedQty / boxQty + 1) * boxQty;
                }

                suggestions.Add(new MonthlyOrderSuggestionDto
                {
                    Label = $"{targetYear}/{targetMonth:D2}",
                    Year = targetYear,
                    Month = targetMonth,
                    SuggestedQty = suggestedQty,
                    EstimatedStock = (int)estimatedStock,
                });

                // 発注した場合、次月の在庫に加算（リードタイム後に入荷と仮定）
                if (suggestedQty > 0)
                    estimatedStock += suggestedQty;
            }

            result.Add(new InventoryOverviewDto
            {
                ProductId = record.ProductId,
                ProductCode = record.Product?.ProductCode ?? string.Empty,
                ProductName = record.Product?.Name ?? string.Empty,
                Unit = record.Product?.Unit ?? string.Empty,
                SixMonthAvgShipment = avgShipment,
                StockStatus = status,
                UpdatedAt = record.UpdatedAt,
                Warehouse89 = warehouseStock.Warehouse89,
                Warehouse81 = warehouseStock.Warehouse81,
                WarehouseInspection = warehouseStock.WarehouseInspection,
                Warehouse4th = warehouseStock.Warehouse4th,
                TotalWarehouseStock = totalWarehouseStock,
                UnallocatedQty = warehouseStock.UnallocatedQty,
                ShippedQty = warehouseStock.ShippedQty,
                SafetyStock = safetyStock,
                TurnoverMonths = turnoverMonths,
                LeadTimeMonths = leadTimeMonths,
                MonthlyOrderSuggestions = suggestions,
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InventoryOverviewExtendedDto>> GetInventoryOverviewExtendedAsync()
    {
        var records = await _repo.GetAllAsync();
        var result = new List<InventoryOverviewExtendedDto>();

        foreach (var record in records)
        {
            // 計算六個月平均出貨量
            var monthlySummaries = await _repo.GetMonthlyShipmentSummaryAsync(record.ProductId, 6);
            var summaryList = monthlySummaries.ToList();
            decimal avgShipment = summaryList.Count > 0
                ? (decimal)summaryList.Sum(s => s.TotalShipped) / summaryList.Count
                : 0;

            // 取得採購建議量（若有）
            var suggestion = await _procurementRepo.GetSuggestionByProductIdAsync(record.ProductId);
            int suggestedQty = suggestion?.SystemSuggestedQty ?? 0;

            // Get warehouse stock
            var warehouseStock = await _warehouseStockRepo.GetByProductIdAsync(record.ProductId);
            int totalWarehouseStock = warehouseStock.Warehouse89 + warehouseStock.Warehouse81 
                + warehouseStock.WarehouseInspection + warehouseStock.Warehouse4th;

            // Get safety stock from product
            int safetyStock = record.Product?.SafetyStock ?? 0;

            // 庫存狀態：低於安全在庫 → Low
            string status = record.CurrentStock < safetyStock ? "Low" : "Normal";

            // Get suppliers for this product
            var currentPrices = (await _supplierRepo.GetCurrentPricesByProductIdAsync(record.ProductId))
                .OrderBy(p => p.UnitPrice).ToList();
            var suppliers = currentPrices.Take(4).Select(p => new SupplierInfoForExportDto
            {
                SupplierName = p.Supplier?.Name ?? "",
                UnitPrice = p.UnitPrice,
                Currency = p.Currency,
                LeadTimeDays = p.LeadTimeDays
            }).ToList();

            // Get monthly shipments for current year
            int currentYear = DateTime.UtcNow.Year;
            var monthlyShipments = await _monthlyShipmentRepo.GetByProductIdAsync(record.ProductId, currentYear);
            var monthlyDict = monthlyShipments.ToDictionary(m => m.Month, m => m.Quantity);

            result.Add(new InventoryOverviewExtendedDto
            {
                ProductId = record.ProductId,
                ProductCode = record.Product?.ProductCode ?? string.Empty,
                ProductName = record.Product?.Name ?? string.Empty,
                Unit = record.Product?.Unit ?? string.Empty,
                SixMonthAvgShipment = avgShipment,
                StockStatus = status,
                UpdatedAt = record.UpdatedAt,
                // Warehouse breakdown
                Warehouse89 = warehouseStock.Warehouse89,
                Warehouse81 = warehouseStock.Warehouse81,
                WarehouseInspection = warehouseStock.WarehouseInspection,
                Warehouse4th = warehouseStock.Warehouse4th,
                TotalWarehouseStock = totalWarehouseStock,
                UnallocatedQty = warehouseStock.UnallocatedQty,
                ShippedQty = warehouseStock.ShippedQty,
                SafetyStock = safetyStock,
                // Extended fields
                BoxQty = record.Product?.BoxQty ?? 1,
                Moq = record.Product?.MOQ ?? 1,
                AverageShipment = record.Product?.AverageShipment ?? 0,
                Suppliers = suppliers,
                MonthlyShipments = monthlyDict
            });
        }

        return result;
    }
}
