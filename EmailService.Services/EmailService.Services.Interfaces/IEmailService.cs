using EmailService.MessageTypes;
using System.ServiceModel;

namespace EmailService.Services.Interfaces
{
    [ServiceContract]
    public interface IEmailService
    {
        [OperationContract]
        void SendEmail(EmailMessage pMessage);
    }
}
