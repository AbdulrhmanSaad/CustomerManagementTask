using CustomersTask4.Messages;
using Microsoft.AspNetCore.SignalR;

namespace CustomersTask4.Hubs
{
    public class MessageHub:Hub
    {

        public async Task SendMessage(string message,string action)
        {
            await Clients.All.SendAsync("ReceiveMessage", message, action);
        }
    }
}
