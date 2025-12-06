using System;
using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.Models
{
    public class SiteSettings
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string? SiteLogo { get; set; }

        // About Us - Multilingual
        [MaxLength(500)]
        public string? AboutUs { get; set; }

        [MaxLength(500)]
        public string? AboutUsEn { get; set; }

        [MaxLength(500)]
        public string? AboutUsKu { get; set; }

        // Contact Information
        [MaxLength(50)]
        public string? ContactPhone { get; set; }

        [MaxLength(50)]
        public string? ContactWhatsApp { get; set; }

        [MaxLength(100)]
        public string? ContactEmail { get; set; }

        // Social Media URLs
        [MaxLength(200)]
        public string? FacebookUrl { get; set; }

        [MaxLength(200)]
        public string? InstagramUrl { get; set; }

        [MaxLength(200)]
        public string? TwitterUrl { get; set; }

        [MaxLength(200)]
        public string? YouTubeUrl { get; set; }

        // Copyright Info - Multilingual
        [MaxLength(500)]
        public string? CopyrightInfo { get; set; }

        [MaxLength(500)]
        public string? CopyrightInfoEn { get; set; }

        [MaxLength(500)]
        public string? CopyrightInfoKu { get; set; }

        // Privacy Policy - Multilingual
        public string? PrivacyPolicy { get; set; }
        public string? PrivacyPolicyEn { get; set; }
        public string? PrivacyPolicyKu { get; set; }

        // Terms of Use - Multilingual
        public string? TermsOfUse { get; set; }
        public string? TermsOfUseEn { get; set; }
        public string? TermsOfUseKu { get; set; }

        // Intellectual Property - Multilingual
        public string? IntellectualProperty { get; set; }
        public string? IntellectualPropertyEn { get; set; }
        public string? IntellectualPropertyKu { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Methods to get content based on current language
        public string? GetPrivacyPolicy(string language)
        {
            return language switch
            {
                "ar" => PrivacyPolicy,
                "en" => PrivacyPolicyEn,
                "ku" => PrivacyPolicyKu,
                _ => PrivacyPolicy // Default to Arabic
            };
        }

        public string? GetTermsOfUse(string language)
        {
            return language switch
            {
                "ar" => TermsOfUse,
                "en" => TermsOfUseEn,
                "ku" => TermsOfUseKu,
                _ => TermsOfUse
            };
        }

        public string? GetIntellectualProperty(string language)
        {
            return language switch
            {
                "ar" => IntellectualProperty,
                "en" => IntellectualPropertyEn,
                "ku" => IntellectualPropertyKu,
                _ => IntellectualProperty
            };
        }

        public string? GetAboutUs(string language)
        {
            return language switch
            {
                "ar" => AboutUs,
                "en" => AboutUsEn,
                "ku" => AboutUsKu,
                _ => AboutUs
            };
        }

        public string? GetCopyrightInfo(string language)
        {
            return language switch
            {
                "ar" => CopyrightInfo,
                "en" => CopyrightInfoEn,
                "ku" => CopyrightInfoKu,
                _ => CopyrightInfo
            };
        }
    }
}