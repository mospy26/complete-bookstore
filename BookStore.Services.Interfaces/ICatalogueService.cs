﻿using System.Collections.Generic;
using System.ServiceModel;
using BookStore.Services.MessageTypes;

namespace BookStore.Services.Interfaces
{
    [ServiceContract]
    public interface ICatalogueService
    {
        [OperationContract]
        List<Book> GetBook(int pOffset, int pCount);

        [OperationContract]
        Book GetBookById(int pId);

        [OperationContract]
        int GetTotalBooks();
    }
}
