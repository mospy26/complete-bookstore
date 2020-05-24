using BookStore.Business.Components.Interfaces;
using System;

namespace BookStore.Business.Components
{
    public class EmailProvider : IEmailProvider
    {
        public void SendMessage(EmailMessage pMessage)
        {
            Console.WriteLine("=============CALLING EMAILSERVICE=============");
            Console.WriteLine("Message: " + pMessage.Message);
            Console.WriteLine("Address: " + pMessage.ToAddress);
            Console.WriteLine("Date: " + DateTime.Now);
            Console.WriteLine("==============================================" + "\n");
            Console.WriteLine(" ");
            ExternalServiceFactory.Instance.EmailService.SendEmail
                (
                    new global::EmailService.MessageTypes.EmailMessage()
                    {
                        Message = pMessage.Message,
                        ToAddresses = pMessage.ToAddress,
                        Date = DateTime.Now
                    }
                );
        }
    }
}
