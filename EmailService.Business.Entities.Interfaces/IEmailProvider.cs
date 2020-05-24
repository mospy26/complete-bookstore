using EmailService.Business.Entities;

namespace EmailService.Business.Components.Interfaces
{
    public interface IEmailProvider
    {
        void SendEmail(EmailMessage pMessage);
    }
}
