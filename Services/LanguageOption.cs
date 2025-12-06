namespace ReverseMarket.Services
{
    public class LanguageOption
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string NativeName { get; set; } = "";
        public string Flag { get; set; } = ""; // إضافة خاصية Flag
        public string Direction { get; set; } = "ltr";
    }
}
