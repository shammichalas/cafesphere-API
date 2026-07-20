namespace CafeSphere.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Preparing = 2,
    Ready = 3,
    Delivered = 4,
    Completed = 5,
    Cancelled = 6
}

public enum OrderType
{
    DineIn = 1,
    Takeaway = 2,
    Delivery = 3
}

public enum PaymentMethod
{
    Cash = 1,
    CreditCard = 2,
    DebitCard = 3,
    UPI = 4,
    Wallet = 5,
    GiftCard = 6
}

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}

public enum ReservationStatus
{
    Confirmed = 1,
    CheckedIn = 2,
    Cancelled = 3,
    NoShow = 4
}

public enum TableStatus
{
    Available = 1,
    Occupied = 2,
    Reserved = 3,
    Maintenance = 4
}

public enum InventoryStatus
{
    InStock = 1,
    LowStock = 2,
    OutOfStock = 3
}

public enum PurchaseOrderStatus
{
    Draft = 1,
    Ordered = 2,
    Received = 3,
    Cancelled = 4
}

public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Late = 3,
    OnLeave = 4
}

public enum CouponType
{
    Percentage = 1,
    FixedAmount = 2
}

public enum NotificationType
{
    Kitchen = 1,
    Pos = 2,
    Inventory = 3,
    System = 4,
    General = 5
}
