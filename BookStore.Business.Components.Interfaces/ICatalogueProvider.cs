using System.Collections.Generic;
using BookStore.Business.Entities;

namespace BookStore.Business.Components.Interfaces
{
    public interface ICatalogueProvider
    {
        List<Business.Entities.Book> GetBook(int pOffset, int pCount);
        Book GetBookById(int pId);

        int GetTotalBooks();
    }
}
