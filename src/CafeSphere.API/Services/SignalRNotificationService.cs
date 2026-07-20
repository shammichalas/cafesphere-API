using CafeSphere.API.Hubs;
using CafeSphere.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CafeSphere.API.Services;

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<KitchenHub> _kitchenHub;
    private readonly IHubContext<PosHub> _posHub;
    private readonly IHubContext<DashboardHub> _dashboardHub;
    private readonly IHubContext<NotificationHub> _notificationHub;

    public SignalRNotificationService(
        IHubContext<KitchenHub> kitchenHub,
        IHubContext<PosHub> posHub,
        IHubContext<DashboardHub> dashboardHub,
        IHubContext<NotificationHub> notificationHub)
    {
        _kitchenHub = kitchenHub;
        _posHub = posHub;
        _dashboardHub = dashboardHub;
        _notificationHub = notificationHub;
    }

    public async Task NotifyKitchenOrderReceivedAsync(object orderDto, CancellationToken cancellationToken = default)
    {
        await _kitchenHub.Clients.All.SendAsync("OrderReceived", orderDto, cancellationToken);
    }

    public async Task NotifyKitchenOrderStatusChangedAsync(string orderId, string newStatus, CancellationToken cancellationToken = default)
    {
        await _kitchenHub.Clients.All.SendAsync("OrderStatusChanged", new { OrderId = orderId, Status = newStatus }, cancellationToken);
        await _posHub.Clients.All.SendAsync("OrderStatusChanged", new { OrderId = orderId, Status = newStatus }, cancellationToken);
    }

    public async Task NotifyPosOrderUpdatedAsync(object orderDto, CancellationToken cancellationToken = default)
    {
        await _posHub.Clients.All.SendAsync("PosOrderUpdated", orderDto, cancellationToken);
    }

    public async Task NotifyDashboardMetricsUpdatedAsync(object metricsDto, CancellationToken cancellationToken = default)
    {
        await _dashboardHub.Clients.All.SendAsync("MetricsUpdated", metricsDto, cancellationToken);
    }

    public async Task SendUserNotificationAsync(string userId, string title, string message, CancellationToken cancellationToken = default)
    {
        await _notificationHub.Clients.All.SendAsync("ReceiveNotification", new { Title = title, Message = message, Timestamp = DateTime.UtcNow }, cancellationToken);
    }
}
