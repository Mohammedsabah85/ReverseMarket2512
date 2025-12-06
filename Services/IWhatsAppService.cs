namespace ReverseMarket.Services
{
    public interface IWhatsAppService
    {
        Task<bool> SendOTPAsync(string phoneNumber, string otp);
        Task<bool> SendLoginOTPAsync(string phoneNumber, string otp, string firstName);
        Task<bool> SendPhoneVerificationAsync(string phoneNumber, string code, bool isExistingUser);
        Task<bool> SendWelcomeMessageAsync(string phoneNumber, string firstName, string userType);
     
        Task<bool> SendWhatsAppNotificationAsync(string phoneNumber, string message); // ✅ جديد

    }
}