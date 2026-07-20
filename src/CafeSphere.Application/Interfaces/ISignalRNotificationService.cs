namespace CafeSphere.Application.Interfaces;

public interface ISignalRNotificationService
{
    Task NotifyKitchenOrderReceivedAsync(object orderDto, CancellationToken cancellationToken = default);
    Task NotifyKitchenOrderStatusChangedAsync(string orderId, string newStatus, CancellationToken cancellationToken = default);
    Task NotifyPosOrderUpdatedAsync(object orderDto, CancellationToken cancellationToken = default);
    Task NotifyDashboardMetricsUpdatedAsync(object metricsDto, CancellationToken cancellationToken = default);
    Task SendUserNotificationAsync(string userId, string title, string message, CancellationToken cancellationToken = default);
}
