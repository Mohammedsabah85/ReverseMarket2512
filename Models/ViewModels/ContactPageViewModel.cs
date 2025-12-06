using ReverseMarket.Controllers;

namespace ReverseMarket.Models.ViewModels
{
    public class ContactPageViewModel
    {
        public SiteSettings SiteSettings { get; set; }
        public List<Advertisement> Advertisements { get; set; }
        public ContactFormModel ContactForm { get; set; } = new ContactFormModel();
    }
}
