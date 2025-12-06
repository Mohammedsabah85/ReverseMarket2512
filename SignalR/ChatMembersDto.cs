namespace ReverseMarket.SignalR
{
    public class ChatMembersDto
    {
        // Id witch is Phone-unique
        public string SenderId { get; set; } // User who sent the message
        public string ReceiverId { get; set; } // User who received the message

        // for display names only : 
        public string SenderFullName { get; set; }
        public string ReceiverFullName { get; set; }
    }
}
