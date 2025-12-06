using Microsoft.AspNetCore.SignalR;

namespace ReverseMarket.SignalR
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            string username = connection.User?.Identity?.Name?.ToLower();
            return username; // lowercase for consistency
        }
    }
}
