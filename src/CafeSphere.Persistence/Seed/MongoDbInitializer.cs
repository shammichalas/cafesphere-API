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
        try
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

            // Product Index
            var productSlugIndex = new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Slug),
                new CreateIndexOptions { Unique = true, Name = "UX_Product_Slug" }
            );
            await context.Products.Indexes.CreateOneAsync(productSlugIndex);

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
        catch
        {
            // Index creation retry safe fallback
        }
    }

    private static async Task SeedDataAsync(IMongoDbContext context)
    {
        // 1. Seed Roles
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

        // 2. Seed System Users (Admin, Cashier, Kitchen Staff)
        var adminUser = await context.Users.Find(u => u.Email == "admin@cafesphere.com").FirstOrDefaultAsync();
        if (adminUser == null)
        {
            var users = new List<User>
            {
                new User
                {
                    Username = "admin",
                    Email = "admin@cafesphere.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    FullName = "Alexandra S. (Manager)",
                    PhoneNumber = "+1234567890",
                    Role = Roles.SuperAdmin,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "cashier",
                    Email = "cashier@cafesphere.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cashier@123"),
                    FullName = "John Doe (Cashier)",
                    PhoneNumber = "+1987654321",
                    Role = Roles.Cashier,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "kitchen",
                    Email = "kitchen@cafesphere.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Kitchen@123"),
                    FullName = "Chef Marco (Kitchen)",
                    PhoneNumber = "+1122334455",
                    Role = Roles.KitchenStaff,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            await context.Users.InsertManyAsync(users);
        }

        // 3. Seed Categories
        var categoryCount = await context.Categories.CountDocumentsAsync(FilterDefinition<Category>.Empty);
        if (categoryCount == 0)
        {
            var categories = new List<Category>
            {
                new Category { Name = "Espresso & Coffee", Slug = "espresso-coffee", Description = "Artisanal espresso beverages and specialty roasts", DisplayOrder = 1, IsActive = true },
                new Category { Name = "Tea & Brews", Slug = "tea-brews", Description = "Organic herbal teas and matcha lattes", DisplayOrder = 2, IsActive = true },
                new Category { Name = "Bakery & Pastries", Slug = "bakery-pastries", Description = "Freshly baked croissants, muffins, and artisan breads", DisplayOrder = 3, IsActive = true },
                new Category { Name = "Breakfast & Sandwiches", Slug = "breakfast-sandwiches", Description = "Gourmet toasts, eggs Benedict, and paninis", DisplayOrder = 4, IsActive = true },
                new Category { Name = "Cold Beverages", Slug = "cold-beverages", Description = "Cold brews, iced coffees, and fruit smoothies", DisplayOrder = 5, IsActive = true }
            };
            await context.Categories.InsertManyAsync(categories);
        }

        // 4. Seed Products
        var productCount = await context.Products.CountDocumentsAsync(FilterDefinition<Product>.Empty);
        if (productCount == 0)
        {
            var coffeeCategory = await context.Categories.Find(c => c.Slug == "espresso-coffee").FirstOrDefaultAsync();
            var bakeryCategory = await context.Categories.Find(c => c.Slug == "bakery-pastries").FirstOrDefaultAsync();
            var breakfastCategory = await context.Categories.Find(c => c.Slug == "breakfast-sandwiches").FirstOrDefaultAsync();

            var products = new List<Product>
            {
                new Product
                {
                    Name = "Artisanal Double Espresso",
                    Slug = "artisanal-double-espresso",
                    Description = "Rich and smooth double shot extracted from single-origin Arabica beans",
                    Price = 3.50m,
                    CostPrice = 0.60m,
                    CategoryId = coffeeCategory?.Id ?? "",
                    CategoryName = "Espresso & Coffee",
                    ImageUrl = "https://images.unsplash.com/photo-1510591509098-f4fdc6d0ff04?w=500",
                    IsAvailable = true,
                    PreparationTimeMinutes = 3
                },
                new Product
                {
                    Name = "Velvety Cappuccino",
                    Slug = "velvety-cappuccino",
                    Description = "Equal parts espresso, steamed milk, and deep milk foam sprinkled with cocoa powder",
                    Price = 4.75m,
                    CostPrice = 0.90m,
                    CategoryId = coffeeCategory?.Id ?? "",
                    CategoryName = "Espresso & Coffee",
                    ImageUrl = "https://images.unsplash.com/photo-1534778101976-62847782c213?w=500",
                    IsAvailable = true,
                    PreparationTimeMinutes = 4
                },
                new Product
                {
                    Name = "Iced Caramel Macchiato",
                    Slug = "iced-caramel-macchiato",
                    Description = "Fresh milk combined with vanilla syrup, topped with espresso and caramel drizzle",
                    Price = 5.50m,
                    CostPrice = 1.10m,
                    CategoryId = coffeeCategory?.Id ?? "",
                    CategoryName = "Espresso & Coffee",
                    ImageUrl = "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=500",
                    IsAvailable = true,
                    PreparationTimeMinutes = 4
                },
                new Product
                {
                    Name = "French Butter Croissant",
                    Slug = "french-butter-croissant",
                    Description = "Flaky, golden-brown puff pastry baked fresh every morning with French butter",
                    Price = 3.95m,
                    CostPrice = 0.80m,
                    CategoryId = bakeryCategory?.Id ?? "",
                    CategoryName = "Bakery & Pastries",
                    ImageUrl = "https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=500",
                    IsAvailable = true,
                    PreparationTimeMinutes = 1
                },
                new Product
                {
                    Name = "Sourdough Avocado Toast",
                    Slug = "sourdough-avocado-toast",
                    Description = "Poached egg, smashed Haas avocado, chili flakes, and microgreens on toasted sourdough",
                    Price = 8.95m,
                    CostPrice = 2.20m,
                    CategoryId = breakfastCategory?.Id ?? "",
                    CategoryName = "Breakfast & Sandwiches",
                    ImageUrl = "https://images.unsplash.com/photo-1525351484163-7529414344d8?w=500",
                    IsAvailable = true,
                    PreparationTimeMinutes = 8
                }
            };
            await context.Products.InsertManyAsync(products);
        }

        // 5. Seed Inventory Items
        var inventoryCount = await context.Inventory.CountDocumentsAsync(FilterDefinition<InventoryItem>.Empty);
        if (inventoryCount == 0)
        {
            var inventoryItems = new List<InventoryItem>
            {
                new InventoryItem { ItemName = "Single-Origin Arabica Coffee Beans", SKU = "INV-BEANS-01", Category = "Coffee", CurrentStock = 45.5, MinimumStock = 10, UnitOfMeasure = "kg", CostPerUnit = 14.50m, Status = InventoryStatus.InStock },
                new InventoryItem { ItemName = "Whole Organic Milk", SKU = "INV-MILK-WHOLE", Category = "Dairy", CurrentStock = 120, MinimumStock = 20, UnitOfMeasure = "L", CostPerUnit = 1.20m, Status = InventoryStatus.InStock },
                new InventoryItem { ItemName = "Unsweetened Almond Milk", SKU = "INV-MILK-ALMOND", Category = "Dairy Alternatives", CurrentStock = 35, MinimumStock = 10, UnitOfMeasure = "L", CostPerUnit = 2.10m, Status = InventoryStatus.InStock },
                new InventoryItem { ItemName = "French Pastry Butter", SKU = "INV-BUTTER-01", Category = "Bakery Ingredients", CurrentStock = 18, MinimumStock = 5, UnitOfMeasure = "kg", CostPerUnit = 6.80m, Status = InventoryStatus.InStock }
            };
            await context.Inventory.InsertManyAsync(inventoryItems);
        }

        // 6. Seed Cafe Tables
        var tableCount = await context.Tables.CountDocumentsAsync(FilterDefinition<Table>.Empty);
        if (tableCount == 0)
        {
            var tables = new List<Table>
            {
                new Table { TableNumber = "T1", Capacity = 2, LocationArea = "Indoor Window", Status = TableStatus.Available },
                new Table { TableNumber = "T2", Capacity = 4, LocationArea = "Main Dining", Status = TableStatus.Occupied },
                new Table { TableNumber = "T3", Capacity = 6, LocationArea = "Patio Terrace", Status = TableStatus.Reserved },
                new Table { TableNumber = "T4", Capacity = 4, LocationArea = "VIP Lounge Booth", Status = TableStatus.Available }
            };
            await context.Tables.InsertManyAsync(tables);
        }

        // 7. Seed Initial Sample Orders
        var orderCount = await context.Orders.CountDocumentsAsync(FilterDefinition<Order>.Empty);
        if (orderCount == 0)
        {
            var sampleOrder = new Order
            {
                OrderNumber = "ORD-2026-1001",
                Type = OrderType.DineIn,
                Status = OrderStatus.Preparing,
                PaymentStatus = PaymentStatus.Completed,
                TableNumber = "T2",
                SubTotal = 14.20m,
                TaxAmount = 1.14m,
                DiscountAmount = 0.00m,
                TotalAmount = 15.34m,
                Notes = "Extra caramel drizzle on cappuccino",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = "prod-1", ProductName = "Velvety Cappuccino", UnitPrice = 4.75m, Quantity = 2 },
                    new OrderItem { ProductId = "prod-2", ProductName = "French Butter Croissant", UnitPrice = 4.70m, Quantity = 1 }
                },
                CreatedAt = DateTime.UtcNow
            };
            await context.Orders.InsertOneAsync(sampleOrder);
        }
    }
}
