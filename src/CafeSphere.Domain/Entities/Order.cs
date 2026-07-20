using CafeSphere.Domain.Common;
using CafeSphere.Domain.Enums;

namespace CafeSphere.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public string CustomerName { get; set; } = "Walk-in Guest";
    public OrderType Type { get; set; } = OrderType.DineIn;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? TableId { get; set; }
    public string? TableNumber { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public string? PaymentId { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public DateTime? PreparationStartedAt { get; set; }
    public DateTime? PreparationCompletedAt { get; set; }
}

public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal => UnitPrice * Quantity;
    public string? SpecialInstructions { get; set; }
}

public class Payment : BaseEntity
{
    public string OrderId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}
