using CafeSphere.Domain.Common;
using CafeSphere.Domain.Enums;

namespace CafeSphere.Domain.Entities;

public class Table : BaseEntity
{
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; } = 4;
    public string LocationArea { get; set; } = "Main Floor";
    public TableStatus Status { get; set; } = TableStatus.Available;
    public string? CurrentOrderId { get; set; }
}

public class Reservation : BaseEntity
{
    public string? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string TableId { get; set; } = string.Empty;
    public string TableNumber { get; set; } = string.Empty;
    public int PartySize { get; set; }
    public DateTime ReservationTime { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;
    public string? SpecialNotes { get; set; }
}

public class Customer : BaseEntity
{
    public string? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int LoyaltyPoints { get; set; } = 0;
    public decimal TotalSpent { get; set; } = 0;
    public string MembershipTier { get; set; } = "Bronze";
    public DateTime? LastVisitDate { get; set; }
}
