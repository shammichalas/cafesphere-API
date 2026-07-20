using AutoMapper;
using CafeSphere.Application.DTOs;
using CafeSphere.Domain.Entities;

namespace CafeSphere.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Role, RoleDto>();
        
        CreateMap<Category, CategoryDto>();
        CreateMap<Product, ProductDto>();
        CreateMap<ProductIngredient, ProductIngredientDto>().ReverseMap();

        CreateMap<Order, OrderDto>();
        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<Payment, PaymentDto>();

        CreateMap<Table, TableDto>();
        CreateMap<Reservation, ReservationDto>();
        CreateMap<Customer, CustomerDto>();

        CreateMap<InventoryItem, InventoryItemDto>();
        CreateMap<Supplier, SupplierDto>();

        CreateMap<Employee, EmployeeDto>();
        CreateMap<Attendance, AttendanceDto>();

        CreateMap<Expense, ExpenseDto>();
        CreateMap<Coupon, CouponDto>();
    }
}

public record RoleDto(
    string Id,
    string Name,
    string Description,
    List<string> Permissions
);
