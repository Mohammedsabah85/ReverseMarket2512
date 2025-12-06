namespace ReverseMarket.Models
{
    public class WhatsAppSettings
    {
        public string Provider { get; set; } = "";
        public string ApiUrl { get; set; } = "";
        public string ApiToken { get; set; } = "";
        public string ApiKey => ApiToken; // للتوافق مع الكود الموجود
        public string DefaultPhone { get; set; } = "";
        public bool EnableNotifications { get; set; } = true;
        public bool EnableFallback { get; set; } = true;
        public string FallbackProvider { get; set; } = "";
    }
}