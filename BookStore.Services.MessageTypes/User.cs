using Microsoft.AspNet.Identity;
using System;

namespace BookStore.Services.MessageTypes
{
    public class User : MessageType, IUser<int>, IUser<string>
    {
        public String Name { get; set; }
        public String Email { get; set; }
        public String Address { get; set; }
        public LoginCredential LoginCredential { get; set; }

        public byte[] Revision { get; set; }


        public string UserName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }

        string IUser<string>.Id
        {
            get { return this.Id.ToString(); }
        }
    }
}
