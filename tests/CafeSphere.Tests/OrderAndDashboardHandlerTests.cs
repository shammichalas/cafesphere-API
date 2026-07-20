using CafeSphere.Application.DTOs;
using CafeSphere.Application.Features.Catalog;
using CafeSphere.Application.Features.Dashboard;
using CafeSphere.Application.Features.Orders;
using CafeSphere.Application.Interfaces;
using CafeSphere.Domain.Entities;
using CafeSphere.Domain.Enums;
using CafeSphere.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace CafeSphere.Tests;

public class OrderAndDashboardHandlerTests
{
    private readonly Mock<IMongoRepository<Order>> _orderRepoMock = new();
    private readonly Mock<IMongoRepository<Product>> _productRepoMock = new();
    private readonly Mock<IMongoRepository<Coupon>> _couponRepoMock = new();
    private readonly Mock<IMongoRepository<InventoryItem>> _inventoryRepoMock = new();
    private readonly Mock<IMongoRepository<Payment>> _paymentRepoMock = new();
    private readonly Mock<ISignalRNotificationService> _signalRMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();

    [Fact]
    public async Task CreateOrder_Should_Calculate_Totals_And_Dispatch_SignalR()
    {
        // Arrange
        var product = new Product
        {
            Id = "prod-1",
            Name = "Velvety Cappuccino",
            Price = 4.75m,
            IsAvailable = true
        };

        _productRepoMock.Setup(r => r.GetByIdAsync("prod-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new CreateOrderCommandHandler(
            _orderRepoMock.Object,
            _productRepoMock.Object,
            _couponRepoMock.Object,
            _signalRMock.Object
        );

        var command = new CreateOrderCommand(
            CustomerId: "cust-1",
            CustomerName: "Alice Smith",
            Type: OrderType.DineIn,
            TableId: "T1",
            CouponCode: null,
            Notes: "Extra froth",
            Items: new List<CreateOrderItemRequest>
            {
                new("prod-1", 2, "Extra froth")
            }
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.SubTotal.Should().Be(9.50m);
        result.Value.TaxAmount.Should().Be(0.76m); // 8% of 9.50
        result.Value.TotalAmount.Should().Be(10.26m);

        _orderRepoMock.Verify(r => r.InsertAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _signalRMock.Verify(s => s.NotifyKitchenOrderReceivedAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateKitchenStatus_Should_Update_State_And_Notify_Hub()
    {
        // Arrange
        var order = new Order
        {
            Id = "ord-123",
            OrderNumber = "ORD-2026-1001",
            Status = OrderStatus.Pending
        };

        _orderRepoMock.Setup(r => r.GetByIdAsync("ord-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new UpdateKitchenOrderStatusCommandHandler(
            _orderRepoMock.Object,
            _signalRMock.Object
        );

        var command = new UpdateKitchenOrderStatusCommand("ord-123", OrderStatus.Preparing);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Preparing);
        order.PreparationStartedAt.Should().NotBeNull();

        _orderRepoMock.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _signalRMock.Verify(s => s.NotifyKitchenOrderStatusChangedAsync("ord-123", "Preparing", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetKitchenQueue_Should_Return_Only_Active_Kitchen_Tickets()
    {
        // Arrange
        var activeOrders = new List<Order>
        {
            new Order { Id = "1", OrderNumber = "ORD-1", Status = OrderStatus.Pending, CustomerName = "User A" },
            new Order { Id = "2", OrderNumber = "ORD-2", Status = OrderStatus.Preparing, CustomerName = "User B" }
        };

        _orderRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeOrders);

        var handler = new GetKitchenQueueQueryHandler(_orderRepoMock.Object);

        // Act
        var result = await handler.Handle(new GetKitchenQueueQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.First().CustomerName.Should().Be("User A");
    }

    [Fact]
    public async Task GetDashboardMetrics_Should_Aggregate_Today_Revenue_And_Top_Products()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order
            {
                Id = "1",
                Status = OrderStatus.Completed,
                PaymentStatus = PaymentStatus.Completed,
                TotalAmount = 50.00m,
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = "p1", ProductName = "Double Espresso", UnitPrice = 3.50m, Quantity = 4 }
                }
            }
        };

        _orderRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var handler = new GetDashboardMetricsQueryHandler(
            _orderRepoMock.Object,
            _productRepoMock.Object,
            _inventoryRepoMock.Object
        );

        // Act
        var result = await handler.Handle(new GetDashboardMetricsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TodayRevenue.Should().Be(50.00m);
        result.Value.TodayOrdersCount.Should().Be(1);
        result.Value.TopProducts.Should().NotBeEmpty();
        result.Value.TopProducts.First().ProductName.Should().Be("Double Espresso");
    }

    [Fact]
    public async Task CheckoutOrder_Should_Generate_Receipt_With_Calculated_Totals()
    {
        // Arrange
        var handler = new CheckoutOrderCommandHandler(
            _orderRepoMock.Object,
            _paymentRepoMock.Object,
            _signalRMock.Object,
            _mediatorMock.Object
        );

        var command = new CheckoutOrderCommand(
            OrderId: null,
            AmountPaid: 20.00m,
            Method: PaymentMethod.CreditCard,
            CustomerName: "John Customer"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.CustomerName.Should().Be("John Customer");
        result.Value.PaymentMethod.Should().Be(PaymentMethod.CreditCard);
        result.Value.TotalAmount.Should().Be(21.60m); // 20 + 8% tax
    }

    [Fact]
    public async Task GetAIRecommendations_Should_Analyze_Sales_Data()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order
            {
                Items = new List<OrderItem> { new OrderItem { ProductName = "Iced Caramel Macchiato", Quantity = 5 } }
            }
        };

        _orderRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        _inventoryRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem>());

        var handler = new GetAIRecommendationsQueryHandler(
            _orderRepoMock.Object,
            _inventoryRepoMock.Object
        );

        // Act
        var result = await handler.Handle(new GetAIRecommendationsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Answer.Should().Contain("Iced Caramel Macchiato");
        result.Value.ActionableSuggestions.Should().NotBeEmpty();
    }
}
