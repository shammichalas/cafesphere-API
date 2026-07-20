namespace CafeSphere.Shared.Constants;

public static class Permissions
{
    public static class Users
    {
        public const string Read = "Permissions.Users.Read";
        public const string Create = "Permissions.Users.Create";
        public const string Update = "Permissions.Users.Update";
        public const string Delete = "Permissions.Users.Delete";
    }

    public static class Orders
    {
        public const string Read = "Permissions.Orders.Read";
        public const string Create = "Permissions.Orders.Create";
        public const string Update = "Permissions.Orders.Update";
        public const string Delete = "Permissions.Orders.Delete";
        public const string ProcessPayment = "Permissions.Orders.ProcessPayment";
    }

    public static class Products
    {
        public const string Read = "Permissions.Products.Read";
        public const string Manage = "Permissions.Products.Manage";
    }

    public static class Inventory
    {
        public const string Read = "Permissions.Inventory.Read";
        public const string Manage = "Permissions.Inventory.Manage";
    }

    public static class Kitchen
    {
        public const string ManageQueue = "Permissions.Kitchen.ManageQueue";
    }

    public static class Dashboard
    {
        public const string ViewAnalytics = "Permissions.Dashboard.ViewAnalytics";
    }
}

public static class AppClaimTypes
{
    public const string UserId = "uid";
    public const string Email = "email";
    public const string Role = "role";
    public const string FullName = "name";
    public const string Permission = "permission";
}
