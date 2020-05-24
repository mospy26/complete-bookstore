using System;

namespace BookStore.WebClient.ViewModels
{
    public class InsufficientStockViewModel
    {
        public InsufficientStockViewModel(String pItemName)
        {
            ItemName = pItemName;
        }

        public String ItemName { get; set; }
    }
}