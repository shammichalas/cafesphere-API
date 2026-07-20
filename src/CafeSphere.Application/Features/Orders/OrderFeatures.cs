using AutoMapper;
using CafeSphere.Application.DTOs;
using CafeSphere.Application.Interfaces;
using CafeSphere.Domain.Entities;
using CafeSphere.Domain.Enums;
using CafeSphere.Domain.Repositories;
using CafeSphere.Shared.Models;
using FluentValidation;
using MediatR;

namespace CafeSphere.Application.Features.Orders;

public record CreateOrderCommand(
    string? CustomerId,
    string CustomerName,
    OrderType Type,
    string? TableId,
    string? CouponCode,
    string? Notes,
    List<CreateOrderItemRequest> Items
) : IRequest<Result<OrderDto>>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Order must contain at least one item.");
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IMongoRepository<Order> _orderRepository;
    private readonly IMongoRepository<Product> _productRepository;
    private readonly IMongoRepository<Coupon> _couponRepository;
    private readonly ISignalRNotificationService _signalRService;
    private readonly IMapper _mapper;

    public CreateOrderCommandHandler(
        IMongoRepository<Order> orderRepository,
        IMongoRepository<Product> productRepository,
        IMongoRepository<Coupon> couponRepository,
        ISignalRNotificationService signalRService,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _couponRepository = couponRepository;
        _signalRService = signalRService;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderItems = new List<OrderItem>();
        decimal subTotal = 0;

        foreach (var itemReq in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemReq.ProductId, cancellationToken);
            if (product == null || !product.IsAvailable)
                return Result<OrderDto>.Failure("Product.NotFound", $"Product '{itemReq.ProductId}' is not available.");

            var itemSubtotal = product.Price * itemReq.Quantity;
            subTotal += itemSubtotal;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = itemReq.Quantity,
                SpecialInstructions = itemReq.SpecialInstructions
            });
        }

        decimal discountAmount = 0;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var coupon = await _couponRepository.FindOneAsync(c => c.Code == request.CouponCode.ToUpper() && c.IsActive, cancellationToken);
            if (coupon != null && subTotal >= coupon.MinimumOrderAmount)
            {
                discountAmount = coupon.Type == CouponType.Percentage
                    ? (subTotal * coupon.Value / 100)
                    : coupon.Value;

                if (coupon.MaxDiscountAmount > 0 && discountAmount > coupon.MaxDiscountAmount)
                    discountAmount = coupon.MaxDiscountAmount;
            }
        }

        decimal taxAmount = (subTotal - discountAmount) * 0.10m; // 10% Tax rate
        decimal totalAmount = (subTotal - discountAmount) + taxAmount;

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            Type = request.Type,
            Status = OrderStatus.Pending,
            TableId = request.TableId,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            CouponCode = request.CouponCode,
            Notes = request.Notes,
            Items = orderItems,
            PaymentStatus = PaymentStatus.Pending
        };

        await _orderRepository.InsertAsync(order, cancellationToken);

        var dto = _mapper.Map<OrderDto>(order);

        // Real-time SignalR notifications
        await _signalRService.NotifyKitchenOrderReceivedAsync(dto, cancellationToken);
        await _signalRService.NotifyPosOrderUpdatedAsync(dto, cancellationToken);

        return Result<OrderDto>.Success(dto);
    }
}

public record GetOrdersQuery(
    int PageNumber = 1,
    int PageSize = 20,
    OrderStatus? Status = null
) : IRequest<Result<PagedResult<OrderDto>>>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<PagedResult<OrderDto>>>
{
    private readonly IMongoRepository<Order> _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(IMongoRepository<Order> orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<OrderDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var pagedOrders = await _orderRepository.GetPagedAsync(
            o => !request.Status.HasValue || o.Status == request.Status.Value,
            request.PageNumber,
            request.PageSize,
            o => o.CreatedAt,
            isDescending: true,
            cancellationToken
        );

        var dtos = _mapper.Map<IReadOnlyList<OrderDto>>(pagedOrders.Items);
        var result = PagedResult<OrderDto>.Create(dtos, pagedOrders.TotalCount, pagedOrders.PageNumber, pagedOrders.PageSize);

        return Result<PagedResult<OrderDto>>.Success(result);
    }
}
