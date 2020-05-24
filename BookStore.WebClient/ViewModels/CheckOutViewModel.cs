using System;
using BookStore.Services.MessageTypes;

namespace BookStore.WebClient.ViewModels
{
    public class CheckOutViewModel
    {
        public CheckOutViewModel(User pUser)
        {
            UserName = pUser.UserName;
            Address = pUser.Address;
        }

        public String UserName { get; set; }

        public String Address { get; set; }
    }
}