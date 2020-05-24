namespace BookStore.Business.Components.Interfaces
{
    public interface IStockProvider
    {
        void SellStock(Business.Entities.Stock pStock, int pQuantity);
    }
}
