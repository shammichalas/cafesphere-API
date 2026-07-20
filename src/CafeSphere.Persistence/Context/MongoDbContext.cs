using CafeSphere.Domain.Entities;
using CafeSphere.Persistence.Configurations;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace CafeSphere.Persistence.Context;

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoClient _client;
    public IMongoDatabase Database { get; }

    static MongoDbContext()
    {
        var conventionPack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true),
            new EnumRepresentationConvention(BsonType.String)
        };
        ConventionRegistry.Register("CafeSphereConventions", conventionPack, _ => true);
    }

    public MongoDbContext(IOptions<MongoDbSettings> settingsOptions)
    {
        var settings = settingsOptions.Value;
        _client = new MongoClient(settings.ConnectionString);
        Database = _client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<User> Users => GetCollection<User>("Users");
    public IMongoCollection<Role> Roles => GetCollection<Role>("Roles");
    public IMongoCollection<Employee> Employees => GetCollection<Employee>("Employees");
    public IMongoCollection<Customer> Customers => GetCollection<Customer>("Customers");
    public IMongoCollection<Product> Products => GetCollection<Product>("Products");
    public IMongoCollection<Category> Categories => GetCollection<Category>("Categories");
    public IMongoCollection<Order> Orders => GetCollection<Order>("Orders");
    public IMongoCollection<Payment> Payments => GetCollection<Payment>("Payments");
    public IMongoCollection<Reservation> Reservations => GetCollection<Reservation>("Reservations");
    public IMongoCollection<Table> Tables => GetCollection<Table>("Tables");
    public IMongoCollection<InventoryItem> Inventory => GetCollection<InventoryItem>("Inventory");
    public IMongoCollection<Supplier> Suppliers => GetCollection<Supplier>("Suppliers");
    public IMongoCollection<PurchaseOrder> PurchaseOrders => GetCollection<PurchaseOrder>("PurchaseOrders");
    public IMongoCollection<Attendance> Attendances => GetCollection<Attendance>("Attendance");
    public IMongoCollection<Payroll> Payrolls => GetCollection<Payroll>("Payroll");
    public IMongoCollection<Expense> Expenses => GetCollection<Expense>("Expenses");
    public IMongoCollection<Coupon> Coupons => GetCollection<Coupon>("Coupons");
    public IMongoCollection<Feedback> Feedbacks => GetCollection<Feedback>("Feedback");
    public IMongoCollection<Notification> Notifications => GetCollection<Notification>("Notifications");
    public IMongoCollection<AuditLog> AuditLogs => GetCollection<AuditLog>("AuditLogs");
    public IMongoCollection<SystemSetting> Settings => GetCollection<SystemSetting>("Settings");

    public IMongoCollection<TEntity> GetCollection<TEntity>(string? name = null)
    {
        return Database.GetCollection<TEntity>(name ?? typeof(TEntity).Name);
    }

    public Task<IClientSessionHandle> StartSessionAsync(CancellationToken cancellationToken = default)
    {
        return _client.StartSessionAsync(cancellationToken: cancellationToken);
    }
}
