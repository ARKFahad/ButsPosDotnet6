using Microsoft.AspNetCore.SignalR;


namespace Application.Core.SignalR
{
    public class BroadcastHub:Hub<IHubClient>
    {
        public string GetConnectionId=>Context.ConnectionId;
    }
}
