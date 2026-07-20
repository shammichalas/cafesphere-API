using CafeSphere.Domain.Common;
using CafeSphere.Domain.Enums;

namespace CafeSphere.Domain.Entities;

public class Employee : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = "Kitchen";
    public string Position { get; set; } = "Chef";
    public DateTime HireDate { get; set; } = DateTime.UtcNow;
    public decimal MonthlySalary { get; set; }
    public decimal HourlyRate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Attendance : BaseEntity
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime? ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }
    public double HoursWorked { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Remarks { get; set; }
}

public class Payroll : BaseEntity
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal Bonus { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetSalary => BaseSalary + Bonus - Deductions;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidDate { get; set; }
}

public class Expense : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Utilities";
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string? ReceiptUrl { get; set; }
}

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CouponType Type { get; set; } = CouponType.Percentage;
    public decimal Value { get; set; }
    public decimal MinimumOrderAmount { get; set; } = 0;
    public decimal MaxDiscountAmount { get; set; } = 0;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    public int UsageLimit { get; set; } = 100;
    public int TimesUsed { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class Feedback : BaseEntity
{
    public string? CustomerId { get; set; }
    public string CustomerName { get; set; } = "Anonymous";
    public string? OrderId { get; set; }
    public int Rating { get; set; } = 5;
    public string Comment { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
}

public class Notification : BaseEntity
{
    public string? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.General;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}

public class AuditLog : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? Parameters { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public long ExecutionTimeMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class SystemSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
}
