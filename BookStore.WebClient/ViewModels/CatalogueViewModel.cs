using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BookStore.Services.Interfaces;
using BookStore.Services.MessageTypes;

namespace BookStore.WebClient.ViewModels
{
    public class CatalogueViewModel
    {

        public decimal SizeOfPagination { get; }
        private int ItemsPerPage = 10; // Item limit per pages
        private int StartingOffSet;

        public CatalogueViewModel(int pStartingOffSet)
        {

            this.StartingOffSet = pStartingOffSet * ItemsPerPage;

            int lTotalBooks = GetTotalBooks;

            // Amount of pages 
            decimal lSizeOfPagination = lTotalBooks / ItemsPerPage;
            
            // Check if an extra page is necessary
            if (lTotalBooks % ItemsPerPage == 0)
            {
                SizeOfPagination = Math.Floor(lSizeOfPagination);
            }
            else
            {
                SizeOfPagination = Math.Floor(lSizeOfPagination) + 1;
            }
        }


        public List<Book> ItemsByOffset
        {
            get
            {
                return CatalogueService.GetBook(StartingOffSet, ItemsPerPage);
            }
        }

        private ICatalogueService CatalogueService
        {
            get
            {
                return ServiceFactory.Instance.CatalogueService;
            }
        }

        public List<Book> Items
        {
            get
            {
                return CatalogueService.GetBook(0, Int32.MaxValue);
            }
        }

        private int GetTotalBooks
        {
            get
            {
                return CatalogueService.GetTotalBooks();
            }
        }
    }
}