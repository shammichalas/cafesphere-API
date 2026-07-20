using CafeSphere.Domain.Entities;
using CafeSphere.Domain.Enums;
using CafeSphere.Shared.Constants;
using CafeSphere.Persistence.Context;
using MongoDB.Driver;

namespace CafeSphere.Persistence.Seed;

public static class MongoDbInitializer
{
    public static async Task InitializeAsync(IMongoDbContext context)
    {
        await CreateIndexesAsync(context);
        await SeedDataAsync(context);
    }

    private static async Task CreateIndexesAsync(IMongoDbContext context)
    {
        // Users Index
        var userEmailIndex = new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true, Name = "UX_User_Email" }
        );
        var userUsernameIndex = new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(u => u.Username),
            new CreateIndexOptions { Unique = true, Name = "UX_User_Username" }
        );
        await context.Users.Indexes.CreateManyAsync(new[] { userEmailIndex, userUsernameIndex });

        // Customer Index
        var customerPhoneIndex = new CreateIndexModel<Customer>(
            Builders<Customer>.IndexKeys.Ascending(c => c.Phone),
            new CreateIndexOptions { Unique = false, Name = "IX_Customer_Phone" }
        );
        await context.Customers.Indexes.CreateOneAsync(customerPhoneIndex);

        // Product Index
        var productSlugIndex = new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.Slug),
            new CreateIndexOptions { Unique = true, Name = "UX_Product_Slug" }
        );
        var productCategoryIndex = new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.CategoryId),
            new CreateIndexOptions { Name = "IX_Product_CategoryId" }
        );
        await context.Products.Indexes.CreateManyAsync(new[] { productSlugIndex, productCategoryIndex });

        // Category Index
        var categorySlugIndex = new CreateIndexModel<Category>(
            Builders<Category>.IndexKeys.Ascending(c => c.Slug),
            new CreateIndexOptions { Unique = true, Name = "UX_Category_Slug" }
        );
        await context.Categories.Indexes.CreateOneAsync(categorySlugIndex);

        // Orders Index
        var orderStatusIndex = new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Combine(
                Builders<Order>.IndexKeys.Ascending(o => o.Status),
                Builders<Order>.IndexKeys.Descending(o => o.CreatedAt)
            ),
            new CreateIndexOptions { Name = "IX_Order_Status_CreatedAt" }
        );
        await context.Orders.Indexes.CreateOneAsync(orderStatusIndex);
    }

    private static async Task SeedDataAsync(IMongoDbContext context)
    {
        // Seed Roles
        var roleCount = await context.Roles.CountDocumentsAsync(FilterDefinition<Role>.Empty);
        if (roleCount == 0)
        {
            var roles = new List<Role>
            {
                new Role { Name = Roles.SuperAdmin, Description = "System Super Administrator with full permissions" },
                new Role { Name = Roles.Admin, Description = "Cafe Administrator with full management permissions" },
                new Role { Name = Roles.Manager, Description = "Cafe Manager overseeing POS, Staff, and Inventory" },
                new Role { Name = Roles.Cashier, Description = "Cashier managing POS checkout and orders" },
                new Role { Name = Roles.KitchenStaff, Description = "Kitchen Staff viewing and updating KDS orders" },
                new Role { Name = Roles.Customer, Description = "Customer placing orders and reservations" }
            };
            await context.Roles.InsertManyAsync(roles);
        }

        // Seed SuperAdmin User
        var adminUser = await context.Users.Find(u => u.Email == "admin@cafesphere.com").FirstOrDefaultAsync();
        if (adminUser == null)
        {
            var superAdmin = new User
            {
                Username = "admin",
                Email = "admin@cafesphere.com",
                // Hashed "Admin@123" using standard BCrypt
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "System Administrator",
                PhoneNumber = "+1234567890",
                Role = Roles.SuperAdmin,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.InsertOneAsync(superAdmin);
        }
    }
}
