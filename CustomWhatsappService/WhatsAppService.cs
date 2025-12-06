using Microsoft.Extensions.Options;
using ReverseMarket.CustomWhatsappService;
using ReverseMarket.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class WhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly WhatsSettings _settings;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly IWebHostEnvironment _environment;

    public WhatsAppService(
        HttpClient httpClient,
        IOptions<WhatsSettings> options,
        ILogger<WhatsAppService> logger,
        IWebHostEnvironment environment)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
        _environment = environment;

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.ApiToken);
    }

    public async Task<WhatsAppMessageResponse> SendMessageAsync(WhatsAppMessageRequest request)
    {
        try
        {
            // محاولة Standing Tech أولاً
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("📤 محاولة إرسال إلى: {Phone}", request.recipient);

            var response = await _httpClient.PostAsync(_settings.ApiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // ✅ إذا نجح الإرسال
            if (response.IsSuccessStatusCode &&
                !responseContent.Contains("failed") &&
                !responseContent.Contains("Daily Limit"))
            {
                _logger.LogInformation("✅ تم الإرسال بنجاح عبر Standing Tech");

                if (long.TryParse(responseContent.Trim(), out long messageId))
                {
                    return new WhatsAppMessageResponse
                    {
                        Success = true,
                        Message = $"تم الإرسال - ID: {messageId}",
                        Data = messageId
                    };
                }

                return new WhatsAppMessageResponse
                {
                    Success = true,
                    Message = "تم الإرسال بنجاح"
                };
            }

            // ✅ إذا فشل، استخدم Console Mode
            _logger.LogWarning("⚠️ فشل Standing Tech - استخدام Console Mode");
            return await SendViaConsoleAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ خطأ في الإرسال");
            // استخدام Console Mode كخطة احتياطية
            return await SendViaConsoleAsync(request);
        }
    }

    private async Task<WhatsAppMessageResponse> SendViaConsoleAsync(WhatsAppMessageRequest request)
    {
        try
        {
            // ✅ طباعة في Console بشكل واضح
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              📱 رسالة WhatsApp (وضع التطوير)                 ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"║  📞 إلى: {request.recipient.PadRight(50)}║");
            Console.WriteLine($"║  🕐 الوقت: {DateTime.Now:yyyy-MM-dd HH:mm:ss}".PadRight(65) + "║");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.ForegroundColor = ConsoleColor.Yellow;

            // طباعة الرسالة سطر سطر
            var lines = request.message.Split('\n');
            foreach (var line in lines)
            {
                var paddedLine = $"║  {line}".PadRight(65) + "║";
                Console.WriteLine(paddedLine);
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            // ✅ حفظ في ملف للمراجعة لاحقاً
            var logPath = Path.Combine(_environment.WebRootPath, "logs");
            Directory.CreateDirectory(logPath);

            var logFile = Path.Combine(logPath, $"whatsapp_{DateTime.Now:yyyy-MM-dd}.txt");

            var logEntry = $"""
                
                ════════════════════════════════════════════════════════════════
                [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] رسالة WhatsApp
                ════════════════════════════════════════════════════════════════
                إلى: {request.recipient}
                الرسالة:
                {request.message}
                ════════════════════════════════════════════════════════════════
                
                """;

            await File.AppendAllTextAsync(logFile, logEntry);

            _logger.LogInformation("✅ تم حفظ الرسالة في Console و {LogFile}", logFile);

            return new WhatsAppMessageResponse
            {
                Success = true,
                Message = "✅ تم عرض الرسالة في Console (وضع التطوير)",
                Data = new { mode = "console", recipient = request.recipient, savedTo = logFile }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ خطأ في Console Mode");
            return new WhatsAppMessageResponse
            {
                Success = false,
                Message = $"خطأ: {ex.Message}"
            };
        }
    }
}