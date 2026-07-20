using CafeSphere.Domain.Enums;

namespace CafeSphere.Application.DTOs;

public record OrderDto(
    string Id,
    string OrderNumber,
    string? CustomerId,
    string CustomerName,
    OrderType Type,
    OrderStatus Status,
    string? TableId,
    string? TableNumber,
    decimal SubTotal,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal TotalAmount,
    string? CouponCode,
    string? Notes,
    List<OrderItemDto> Items,
    PaymentStatus PaymentStatus,
    DateTime CreatedAt
);

public record OrderItemDto(
    string ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal SubTotal,
    string? SpecialInstructions
);

public record CreateOrderRequest(
    string? CustomerId,
    string CustomerName,
    OrderType Type,
    string? TableId,
    string? CouponCode,
    string? Notes,
    List<CreateOrderItemRequest> Items
);

public record CreateOrderItemRequest(
    string ProductId,
    int Quantity,
    string? SpecialInstructions
);

public record UpdateOrderStatusRequest(
    OrderStatus NewStatus
);

public record CheckoutRequest(
    string OrderId,
    PaymentMethod Method,
    decimal AmountPaid
);

public record PaymentDto(
    string Id,
    string OrderId,
    string OrderNumber,
    PaymentMethod Method,
    PaymentStatus Status,
    decimal Amount,
    string? TransactionId,
    DateTime PaymentDate
);

public record ReceiptDto(
    string OrderNumber,
    string CustomerName,
    DateTime OrderDate,
    List<OrderItemDto> Items,
    decimal SubTotal,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal TotalAmount,
    PaymentMethod PaymentMethod,
    string ReceiptHeader,
    string ReceiptFooter
);
