using System.Net;
using System.Net.Mail;
using System.Text;

namespace CodePulse.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpClientFactory _httpClientFactory;

        public NotificationService(
            IConfiguration configuration,
            IWebHostEnvironment env,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _env = env;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendWelcomeEmailAsync(string email, string name)
        {
            var fromEmail = _configuration["Smtp:FromEmail"];
            var appPassword = _configuration["Smtp:AppPassword"];

            var templatePath = Path.Combine(_env.ContentRootPath, "HTMLTemplate", "EmailTemplate.html");
            var htmlBody = await File.ReadAllTextAsync(templatePath);
            htmlBody = htmlBody.Replace("#Name#", name);

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, "CodePulse"),
                Subject = "Welcome to CodePulse! 🚀",
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(email));

            using var client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }

        public async Task SendWelcomeSmsAsync(string phoneNumber, string name)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return;

            var templatePath = Path.Combine(_env.ContentRootPath, "SMSTemplate", "Welcome SMS.txt");
            var smsText = await File.ReadAllTextAsync(templatePath);
            smsText = smsText.Replace("#Name#", name);

            var apiKey = _configuration["Sms:Fast2SmsApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                await SendViaFast2SmsAsync(phoneNumber, smsText, apiKey);
                return;
            }

            // Fallback: try Twilio if configured
            var twilioAccountSid = _configuration["Twilio:AccountSid"];
            var twilioAuthToken = _configuration["Twilio:AuthToken"];
            var twilioFromNumber = _configuration["Twilio:FromNumber"];
            if (!string.IsNullOrWhiteSpace(twilioAccountSid) &&
                !string.IsNullOrWhiteSpace(twilioAuthToken) &&
                !string.IsNullOrWhiteSpace(twilioFromNumber))
            {
                await SendViaTwilioAsync(phoneNumber, smsText,
                    twilioAccountSid, twilioAuthToken, twilioFromNumber);
                return;
            }

            // Log a warning so the developer knows SMS is not configured
            Console.WriteLine($"[SMS] No SMS provider configured. Would send to {phoneNumber}: {smsText}");
        }

        private async Task SendViaFast2SmsAsync(string phoneNumber, string message, string apiKey)
        {
            using var client = _httpClientFactory.CreateClient("Fast2Sms");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiKey);

            var payload = new
            {
                sender_id = "TXTIND",
                message = message,
                language = "english",
                route = "q",
                numbers = phoneNumber
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://www.fast2sms.com/dev/bulkV2", content);
            response.EnsureSuccessStatusCode();
        }

        private async Task SendViaTwilioAsync(string phoneNumber, string message,
            string accountSid, string authToken, string fromNumber)
        {
            // Twilio requires the Twilio NuGet package.
            // This method uses the REST API directly via HttpClient to avoid an extra dependency.
            using var client = _httpClientFactory.CreateClient("Twilio");
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {credentials}");

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("To", phoneNumber),
                new KeyValuePair<string, string>("From", fromNumber),
                new KeyValuePair<string, string>("Body", message)
            });

            var response = await client.PostAsync(
                $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json",
                formData);
            response.EnsureSuccessStatusCode();
        }
    }
}
