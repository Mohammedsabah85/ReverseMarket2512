using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ReverseMarket.Services
{
    public class TwilioWhatsapp
    {
        private readonly string accountSid = "ACdbfc9f69804a645610bd6994a9dec182";
        private readonly string authToken = "15c124c22099066cdcfa159f3d4ae35d";

        public TwilioWhatsapp()
        {
            TwilioClient.Init(accountSid, authToken);
        }

        public string SendWhatsAppMessage(string toPhoneNumber, string messageBody)
        {
            var to = new PhoneNumber($"whatsapp:{toPhoneNumber}");
            var from = new PhoneNumber("whatsapp:+14155238886"); // Twilio Sandbox WhatsApp number

            var message = MessageResource.Create(
                body: messageBody,
                from: from,
                to: to
            );
            return message.Sid;
        }
    }
}