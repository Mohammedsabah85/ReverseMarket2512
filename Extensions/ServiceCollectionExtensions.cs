using ReverseMarket.Models;
using ReverseMarket.Services;
using Microsoft.AspNetCore.Identity;
using ReverseMarket.Models.Identity;
using ReverseMarket.Data;

namespace ReverseMarket.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // إضافة إعدادات WhatsApp
            services.Configure<WhatsAppSettings>(configuration.GetSection("WhatsApp"));

            // إضافة الخدمات المخصصة
           // services.AddScoped<IWhatsAppService, WhatsAppService>();
            services.AddScoped<ILanguageService, LanguageService>();

            return services;
        }
    }
}



//using ReverseMarket.Services;

//namespace ReverseMarket.Extensions
//{
//    public static class ServiceCollectionExtensions
//    {
//        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
//        {
//            // Configure settings
//            services.Configure<WhatsAppSettings>(configuration.GetSection("WhatsAppSettings"));
//            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

//            // Add services
//            services.AddScoped<IWhatsAppService, WhatsAppService>();
//            services.AddScoped<IFileService, FileService>();
//            services.AddScoped<IEmailService, EmailService>();

//            // Add HttpClient for WhatsApp service
//            services.AddHttpClient<WhatsAppService>();

//            return services;
//        }
//    }
//}