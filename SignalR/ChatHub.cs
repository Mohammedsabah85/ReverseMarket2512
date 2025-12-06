using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using ReverseMarket.Data;

namespace ReverseMarket.SignalR
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string receiver, string message)
        {
            var sender = Context.User.Identity.Name.ToLower();

            // normalize receiver phone number
            int index = receiver.IndexOf("964");
            string result = index >= 0 ? receiver.Substring(index) : receiver;
            var receiverPhone = $"+{result.Trim().ToLower()}";

            var chatMsg = new ChatMessage
            {
                SenderId = sender,
                ReceiverId = receiverPhone,
                Message = message,
                SentAt = DateTime.Now,
            };

            await _context.ChatMessages.AddAsync(chatMsg);
            await _context.SaveChangesAsync();

            // Only send to receiver
            await Clients.User(receiverPhone)
                .SendAsync("ReceiveMessage", sender, message, chatMsg.SentAt.ToShortTimeString(), null, null);
        }

        public async Task Typing(string receiverUserName)
        {
            var senderUserName = Context.User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(senderUserName) && !string.IsNullOrWhiteSpace(receiverUserName))
            {
                await Clients.User($"+{receiverUserName.Trim().ToLower()}").SendAsync("UserTyping", senderUserName);
            }
        }

        public async Task StopTyping(string receiverUserName)
        {
            var senderUserName = Context.User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(senderUserName) && !string.IsNullOrWhiteSpace(receiverUserName))
            {
                // cuz withuser comming like this "&#x2B;9647700227210"
            int index = receiverUserName.IndexOf("964");

                string result = index >= 0 ? receiverUserName.Substring(index) : receiverUserName;
                // then shaping to interact with db
                await Clients.User($"+{result.Trim().ToLower()}").SendAsync("UserStoppedTyping", senderUserName);
            }
        }

        public async Task MarkAsRead(string senderUserName)
        {
            var currentUser = Context.User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(currentUser) || string.IsNullOrWhiteSpace(senderUserName))
                return;

            // cuz withuser comming like this "&#x2B;9647700227210"
            int index = senderUserName.IndexOf("964");

            string result = index >= 0 ? senderUserName.Substring(index) : senderUserName;
            senderUserName = $"+{result.Trim().ToLower()}";

            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderId == senderUserName && m.ReceiverId == currentUser && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                    msg.IsRead = true;

                await _context.SaveChangesAsync();

                await Clients.User(senderUserName).SendAsync("MessagesRead", currentUser);
            }
        }
    }
}
