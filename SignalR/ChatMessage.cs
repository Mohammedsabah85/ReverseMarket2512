using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.SignalR
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public string? FilePath { get; set; } // Optional
        public string? FileType { get; set; } // Optional
    }
}
