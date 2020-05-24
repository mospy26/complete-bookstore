using BookStore.Business.Components.Interfaces;
using System;

namespace BookStore.Business.Components
{
    public class StockProvider : IStockProvider
    {
        public void SellStock(Entities.Stock pStock, int pQuantity)
        {
            throw new NotImplementedException();
        }
    }
}
