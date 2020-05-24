using System;

namespace BookStore.Business.Components.Interfaces
{
    public class EmailMessage
    {
        public String ToAddress { get; set; }
        public String Message { get; set; }
    }

    public interface IEmailProvider
    {
        void SendMessage(EmailMessage pMessage);
    }
}
