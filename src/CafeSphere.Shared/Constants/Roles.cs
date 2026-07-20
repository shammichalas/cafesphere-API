namespace CafeSphere.Shared.Constants;

public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";
    public const string KitchenStaff = "KitchenStaff";
    public const string Customer = "Customer";

    public static readonly IReadOnlyList<string> All = new[]
    {
        SuperAdmin,
        Admin,
        Manager,
        Cashier,
        KitchenStaff,
        Customer
    };
}
