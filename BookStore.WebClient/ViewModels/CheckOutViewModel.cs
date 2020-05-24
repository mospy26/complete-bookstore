using BookStore.Services.MessageTypes;
using System;

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