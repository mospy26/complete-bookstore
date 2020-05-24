using System;

namespace BookStore.Services.MessageTypes
{
    public class LoginCredential : MessageType
    {
        public String UserName { get; set; }
        public String EncryptedPassword { get; set; }
    }
}
