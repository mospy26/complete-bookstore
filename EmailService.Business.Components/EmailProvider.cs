﻿using EmailService.Business.Components.Interfaces;
using System;

namespace EmailService.Business.Components
{
    public class EmailProvider : IEmailProvider
    {
        public void SendEmail(EmailService.Business.Entities.EmailMessage pMessage)
        {
            Console.WriteLine("==============Email Log==============");
            Console.WriteLine("Email service invoked");
            Console.WriteLine("Address: " + pMessage.ToAddresses);
            Console.WriteLine("Message: " + pMessage.Message);
            Console.WriteLine("Status: SUCCESS");
            Console.WriteLine("=====================================");
            Console.WriteLine(" ");
            Console.WriteLine("Sending email to " + pMessage.ToAddresses + ": " + pMessage.Message);
        }
    }
}
