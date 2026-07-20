using CafeSphere.Domain.Common;
using CafeSphere.Domain.Enums;

namespace CafeSphere.Domain.Entities;

public class InventoryItem : BaseEntity
{
    public string ItemName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double CurrentStock { get; set; }
    public double MinimumStock { get; set; }
    public string UnitOfMeasure { get; set; } = "kg";
    public decimal CostPerUnit { get; set; }
    public InventoryStatus Status { get; set; } = InventoryStatus.InStock;
    public string? SupplierId { get; set; }
    public string? SupplierName { get; set; }
}

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = "Net 30";
    public bool IsActive { get; set; } = true;
}

public class PurchaseOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public List<PurchaseOrderItem> Items { get; set; } = new();
    public decimal TotalCost { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
}

public class PurchaseOrderItem
{
    public string InventoryItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public double QuantityOrdered { get; set; }
    public double QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost => (decimal)QuantityOrdered * UnitCost;
}
