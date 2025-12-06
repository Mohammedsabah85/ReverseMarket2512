using Microsoft.Extensions.Options;
using ReverseMarket.Models;
using System.Text;
using System.Text.Json;

namespace ReverseMarket.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly WhatsAppSettings _settings;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(
            HttpClient httpClient,
            IOptions<WhatsAppSettings> settings,
            ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> SendOTPAsync(string phoneNumber, string otp)
        {
            var message = $"رمز التحقق الخاص بك: {otp}\nصالح لمدة 5 دقائق\nالسوق العكسي";
            return await SendMessageAsync(phoneNumber, message);
        }

        public async Task<bool> SendPhoneVerificationAsync(string phoneNumber, string code, bool isExistingUser)
        {
            var message = isExistingUser
                ? $"مرحباً بعودتك!\nرمز التحقق: {code}\nصالح لمدة 5 دقائق\nالسوق العكسي"
                : $"مرحباً بك في السوق العكسي!\nرمز التحقق: {code}\nصالح لمدة 5 دقائق";

            return await SendMessageAsync(phoneNumber, message);
        }

        public async Task<bool> SendLoginOTPAsync(string phoneNumber, string otp, string userName)
        {
            var message = $"مرحباً {userName}!\nرمز تسجيل الدخول: {otp}\nصالح لمدة 5 دقائق\nالسوق العكسي";
            return await SendMessageAsync(phoneNumber, message);
        }

        public async Task<bool> SendWelcomeMessageAsync(string phoneNumber, string userName, string userType)
        {
            var roleText = userType == "Seller" ? "بائع" : "مشتري";
            var message = $"🎉 مرحباً {userName}!\n\nتم إنشاء حسابك بنجاح كـ {roleText}\n\nيمكنك الآن الاستفادة من جميع خدمات السوق العكسي\n\nشكراً لانضمامك إلينا!";

            return await SendMessageAsync(phoneNumber, message);
        }

        // ✅ Method جديدة للإشعارات
        public async Task<bool> SendWhatsAppNotificationAsync(string phoneNumber, string message)
        {
            try
            {
                _logger.LogInformation("📤 إرسال إشعار واتساب إلى {PhoneNumber}", phoneNumber);

                var messageText = $"{message}\n\nالسوق العكسي";

                var request = new
                {
                    to = phoneNumber,
                    message = messageText
                };

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");

                var response = await _httpClient.PostAsync(_settings.ApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ تم إرسال إشعار واتساب بنجاح إلى {PhoneNumber}", phoneNumber);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ فشل إرسال إشعار واتساب: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطأ في إرسال إشعار واتساب إلى {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        private async Task<bool> SendMessageAsync(string phoneNumber, string message)
        {
            try
            {
                _logger.LogInformation("Sending WhatsApp message to {PhoneNumber}", phoneNumber);

                var request = new
                {
                    to = phoneNumber,
                    message = message
                };

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");

                var response = await _httpClient.PostAsync(_settings.ApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ WhatsApp message sent successfully to {PhoneNumber}", phoneNumber);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Failed to send WhatsApp message: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception sending WhatsApp message to {PhoneNumber}", phoneNumber);
                return false;
            }
        }
    }
}
//using Microsoft.Extensions.Options;
//using ReverseMarket.Models;
//using System.Text;
//using System.Text.Json;
//namespace ReverseMarket.Services
//{
//    public class WhatsAppService : IWhatsAppService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly WhatsAppSettings _settings;
//        private readonly ILogger<WhatsAppService> _logger;

//        public WhatsAppService(
//            HttpClient httpClient,
//            IOptions<WhatsAppSettings> settings,
//            ILogger<WhatsAppService> logger)
//        {
//            _httpClient = httpClient;
//            _settings = settings.Value;
//            _logger = logger;
//        }
//        public async Task<bool> SendOTPAsync(string phoneNumber, string otp)
//        {
//            try
//            {
//                // تطبيق إرسال الرسائل عبر WhatsApp API
//                // يمكن استخدام خدمات مثل Twilio, Green API, إلخ
//                _logger.LogInformation($"إرسال OTP {otp} إلى {phoneNumber}");

//                // محاكاة الإرسال
//                await Task.Delay(100);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"فشل في إرسال OTP إلى {phoneNumber}");
//                return false;
//            }
//        }

//        public async Task<bool> SendWhatsAppNotificationAsync(string phoneNumber, string message)
//        {
//            try
//            {
//                _logger.LogInformation("📤 إرسال إشعار واتساب إلى {PhoneNumber}", phoneNumber);

//                var messageText = $"{message}\n\nالسوق العكسي";

//                var request = new
//                {
//                    to = phoneNumber,
//                    message = messageText
//                };

//                var jsonContent = System.Text.Json.JsonSerializer.Serialize(request);
//                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//                _httpClient.DefaultRequestHeaders.Clear();
//                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiToken}");

//                var response = await _httpClient.PostAsync(_settings.ApiUrl, content);

//                if (response.IsSuccessStatusCode)
//                {
//                    _logger.LogInformation("✅ تم إرسال إشعار واتساب بنجاح إلى {PhoneNumber}", phoneNumber);
//                    return true;
//                }

//                var errorContent = await response.Content.ReadAsStringAsync();
//                _logger.LogError("❌ فشل إرسال إشعار واتساب: {StatusCode} - {Error}",
//                    response.StatusCode, errorContent);
//                return false;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "❌ خطأ في إرسال إشعار واتساب إلى {PhoneNumber}", phoneNumber);
//                return false;
//            }
//        }
//        public async Task<bool> SendLoginOTPAsync(string phoneNumber, string otp, string firstName)
//        {
//            try
//            {
//                var message = $"مرحباً {firstName}!\nرمز الدخول: {otp}\nصالح لمدة 5 دقائق.";
//                _logger.LogInformation($"إرسال رمز دخول إلى {phoneNumber}: {message}");

//                await Task.Delay(100);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"فشل في إرسال رمز الدخول إلى {phoneNumber}");
//                return false;
//            }
//        }

//        public async Task<bool> SendPhoneVerificationAsync(string phoneNumber, string code, bool isExistingUser)
//        {
//            try
//            {
//                var message = isExistingUser
//                    ? $"رمز تأكيد الهاتف: {code}\nصالح لمدة 5 دقائق."
//                    : $"أهلاً بك في السوق العكسي!\nرمز التأكيد: {code}\nصالح لمدة 5 دقائق.";

//                _logger.LogInformation($"إرسال رمز تأكيد إلى {phoneNumber}: {message}");

//                await Task.Delay(100);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"فشل في إرسال رمز التأكيد إلى {phoneNumber}");
//                return false;
//            }
//        }

//        public async Task<bool> SendWelcomeMessageAsync(string phoneNumber, string firstName, string userType)
//        {
//            try
//            {
//                var userTypeText = userType == "Seller" ? "بائع" : "مشتري";
//                var message = $"مرحباً {firstName}!\nتم إنشاء حسابك كـ{userTypeText} بنجاح في السوق العكسي.\nنتمنى لك تجربة ممتعة!";

//                _logger.LogInformation($"إرسال رسالة ترحيب إلى {phoneNumber}: {message}");

//                await Task.Delay(100);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"فشل في إرسال رسالة الترحيب إلى {phoneNumber}");
//                return false;
//            }
//        }
//    }
//}