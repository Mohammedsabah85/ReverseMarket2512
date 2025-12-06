namespace ReverseMarket.CustomWhatsappService
{
    public class WhatsAppMessageRequest
    {
        public string recipient { get; set; }
        public string sender_id { get; set; } = "AliJamal";
        public string type { get; set; } = "whatsapp"; // default whatsapp
        public string message { get; set; }
        public string lang { get; set; } = "en"; // default en
    }
}
