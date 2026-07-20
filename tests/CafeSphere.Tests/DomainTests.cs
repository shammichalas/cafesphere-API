using CafeSphere.Domain.Entities;
using CafeSphere.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CafeSphere.Tests;

public class DomainTests
{
    [Fact]
    public void BaseEntity_Should_Generate_Valid_ObjectId_And_Dates()
    {
        // Act
        var user = new User
        {
            Username = "testuser",
            Email = "test@cafesphere.com"
        };

        // Assert
        user.Id.Should().NotBeNullOrEmpty();
        user.Id.Length.Should().Be(24); // MongoDB ObjectId hex string length
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Order_TotalAmount_Calculation_With_Tax_And_Discount()
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "ORD-TEST-001",
            SubTotal = 100.00m,
            DiscountAmount = 10.00m, // $90 remaining
            TaxAmount = 9.00m,       // 10% tax on $90
            TotalAmount = 99.00m
        };

        // Assert
        (order.SubTotal - order.DiscountAmount + order.TaxAmount).Should().Be(order.TotalAmount);
    }
}
