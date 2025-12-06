using Microsoft.AspNetCore.Localization;
namespace ReverseMarket.Services

{

    //public interface ILanguageService
    //{
    //    string GetCurrentLanguage();
    //    string GetDirection();
    //    List<LanguageOption> GetSupportedLanguages();
    //    void SetLanguage(string languageCode);
    //    bool IsLanguageSupported(string languageCode);
    //}
    public interface ILanguageService
    {
        string GetCurrentLanguage();
        string GetDirection();
        List<LanguageInfo> GetSupportedLanguages();
    }

    public class LanguageInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NativeName { get; set; }
        public string Flag { get; set; }
    }

}