using CafeSphere.Domain.Entities;
using MongoDB.Driver;

namespace CafeSphere.Persistence.Context;

public interface IMongoDbContext
{
    IMongoDatabase Database { get; }
    IMongoCollection<User> Users { get; }
    IMongoCollection<Role> Roles { get; }
    IMongoCollection<Employee> Employees { get; }
    IMongoCollection<Customer> Customers { get; }
    IMongoCollection<Product> Products { get; }
    IMongoCollection<Category> Categories { get; }
    IMongoCollection<Order> Orders { get; }
    IMongoCollection<Payment> Payments { get; }
    IMongoCollection<Reservation> Reservations { get; }
    IMongoCollection<Table> Tables { get; }
    IMongoCollection<InventoryItem> Inventory { get; }
    IMongoCollection<Supplier> Suppliers { get; }
    IMongoCollection<PurchaseOrder> PurchaseOrders { get; }
    IMongoCollection<Attendance> Attendances { get; }
    IMongoCollection<Payroll> Payrolls { get; }
    IMongoCollection<Expense> Expenses { get; }
    IMongoCollection<Coupon> Coupons { get; }
    IMongoCollection<Feedback> Feedbacks { get; }
    IMongoCollection<Notification> Notifications { get; }
    IMongoCollection<AuditLog> AuditLogs { get; }
    IMongoCollection<SystemSetting> Settings { get; }

    IMongoCollection<TEntity> GetCollection<TEntity>(string? name = null);
    Task<IClientSessionHandle> StartSessionAsync(CancellationToken cancellationToken = default);
}
