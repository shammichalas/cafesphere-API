using CafeSphere.Domain.Enums;

namespace CafeSphere.Application.DTOs;

public record TableDto(
    string Id,
    string TableNumber,
    int Capacity,
    string LocationArea,
    TableStatus Status,
    string? CurrentOrderId
);

public record ReservationDto(
    string Id,
    string? CustomerId,
    string CustomerName,
    string CustomerPhone,
    string CustomerEmail,
    string TableId,
    string TableNumber,
    int PartySize,
    DateTime ReservationTime,
    ReservationStatus Status,
    string? SpecialNotes
);

public record CreateReservationRequest(
    string CustomerName,
    string CustomerPhone,
    string CustomerEmail,
    string TableId,
    int PartySize,
    DateTime ReservationTime,
    string? SpecialNotes
);

public record CustomerDto(
    string Id,
    string FullName,
    string Phone,
    string Email,
    int LoyaltyPoints,
    decimal TotalSpent,
    string MembershipTier,
    DateTime? LastVisitDate
);

public record InventoryItemDto(
    string Id,
    string ItemName,
    string SKU,
    string Category,
    double CurrentStock,
    double MinimumStock,
    string UnitOfMeasure,
    decimal CostPerUnit,
    InventoryStatus Status,
    string? SupplierName
);

public record SupplierDto(
    string Id,
    string Name,
    string ContactPerson,
    string Email,
    string Phone,
    string Address,
    string PaymentTerms,
    bool IsActive
);

public record EmployeeDto(
    string Id,
    string UserId,
    string EmployeeCode,
    string FullName,
    string Department,
    string Position,
    DateTime HireDate,
    decimal MonthlySalary,
    decimal HourlyRate,
    bool IsActive
);

public record AttendanceDto(
    string Id,
    string EmployeeId,
    string EmployeeName,
    DateTime Date,
    DateTime? ClockIn,
    DateTime? ClockOut,
    double HoursWorked,
    AttendanceStatus Status,
    string? Remarks
);

public record ExpenseDto(
    string Id,
    string Title,
    string Category,
    decimal Amount,
    DateTime ExpenseDate,
    string Description,
    string ApprovedBy,
    string? ReceiptUrl
);

public record CouponDto(
    string Id,
    string Code,
    string Description,
    CouponType Type,
    decimal Value,
    decimal MinimumOrderAmount,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
);

public record DashboardMetricsDto(
    decimal TodayRevenue,
    long TodayOrdersCount,
    decimal MonthRevenue,
    decimal TotalExpenses,
    decimal NetProfit,
    int ActiveTablesCount,
    int PendingKitchenOrdersCount,
    List<TopProductDto> TopProducts
);

public record TopProductDto(
    string ProductId,
    string ProductName,
    int TotalQuantitySold,
    decimal TotalRevenue
);

public record AIRecommendationRequest(
    string Query,
    string ContextType
);

public record AIRecommendationResponse(
    string Answer,
    List<string> ActionableSuggestions,
    object? DataPayload
);
