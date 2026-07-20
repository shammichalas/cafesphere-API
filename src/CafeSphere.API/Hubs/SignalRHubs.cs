using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CafeSphere.API.Hubs;

[Authorize]
public class KitchenHub : Hub
{
    public async Task JoinKitchenGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "KitchenStaff");
    }

    public async Task LeaveKitchenGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "KitchenStaff");
    }
}

[Authorize]
public class PosHub : Hub
{
    public async Task JoinPosGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "POS");
    }
}

[Authorize]
public class DashboardHub : Hub
{
    public async Task JoinDashboardGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Dashboard");
    }
}

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
        await base.OnConnectedAsync();
    }
}
