using Microsoft.AspNetCore.SignalR;

namespace CSAT_BMTT.Hubs
{
    public class PermissionHub : Hub
    {
        public async Task NotifyRequestUpdated()
        {
            await Clients.All.SendAsync("UpdateRequests");
        }
    }
}
