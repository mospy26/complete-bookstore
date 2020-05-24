using Microsoft.AspNet.Identity;
using System;

namespace BookStore.Services.MessageTypes
{
    public class Role : MessageType, IRole<int>
    {
        public String Name { get; set; }
    }
}
