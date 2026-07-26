namespace CodePulse.API.Services
{
    public interface INotificationService
    {
        Task SendWelcomeEmailAsync(string email, string name);
        Task SendWelcomeSmsAsync(string phoneNumber, string name);
    }
}
