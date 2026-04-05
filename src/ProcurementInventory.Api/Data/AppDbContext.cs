using Microsoft.EntityFrameworkCore;
using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Data;

/// <summary>
/// 應用程式資料庫 Context
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<ProductSupplierPrice> ProductSupplierPrices => Set<ProductSupplierPrice>();
    public DbSet<InventoryRecord> InventoryRecords => Set<InventoryRecord>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<ProcurementSuggestion> ProcurementSuggestions => Set<ProcurementSuggestion>();
    public DbSet<ProcurementSettings> ProcurementSettings => Set<ProcurementSettings>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<AuditDiscrepancyLog> AuditDiscrepancyLogs => Set<AuditDiscrepancyLog>();
    public DbSet<DemandForecast> DemandForecasts => Set<DemandForecast>();
    public DbSet<MonthlyShipment> MonthlyShipments => Set<MonthlyShipment>();
    public DbSet<WarehouseStock> WarehouseStocks => Set<WarehouseStock>();
    public DbSet<MonthlyInventory> MonthlyInventories => Set<MonthlyInventory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProductCode).IsUnique();
            entity.Property(e => e.ProductCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(20);
            entity.Property(e => e.AverageShipment).HasColumnType("decimal(18,4)");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContactInfo).HasMaxLength(500);
        });

        modelBuilder.Entity<ProductSupplierPrice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,4)");
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(10);

            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Supplier)
                  .WithMany(s => s.Prices)
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InventoryRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProductId).IsUnique();

            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.OperatorAccount).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Remark).HasMaxLength(500);

            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProcurementSuggestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProductId).IsUnique();
            entity.Property(e => e.SixMonthAvgShipment).HasColumnType("decimal(18,4)");
            entity.Property(e => e.TurnoverMonths).HasColumnType("decimal(4,2)");

            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProcurementSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DefaultTurnoverMonths).HasColumnType("decimal(4,2)");
            entity.Property(e => e.UpdatedBy).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,4)");
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Supplier)
                  .WithMany()
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,4)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,4)");

            entity.HasOne(e => e.PurchaseOrder)
                  .WithMany(o => o.Items)
                  .HasForeignKey(e => e.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditDiscrepancyLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DemandForecast>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ForecastQty).HasColumnType("decimal(18,4)");
            entity.Property(e => e.ConfidenceLower).HasColumnType("decimal(18,4)");
            entity.Property(e => e.ConfidenceUpper).HasColumnType("decimal(18,4)");
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MonthlyShipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProductId, e.Year, e.Month }).IsUnique();
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WarehouseStock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProductId).IsUnique();
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MonthlyInventory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProductId, e.Year, e.Month }).IsUnique();
            entity.Property(e => e.StockAmount).HasColumnType("decimal(18,4)");
            entity.Property(e => e.TurnoverRate).HasColumnType("decimal(10,4)");
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
