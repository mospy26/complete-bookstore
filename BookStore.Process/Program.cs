﻿using BookStore.Business.Components.Interfaces;
using BookStore.Business.Entities;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Practices.Unity.ServiceLocatorAdapter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Transactions;

namespace BookStore.Process
{
    public class Program
    {
        static void Main(string[] args)
        {
            ResolveDependencies();
            InsertDummyEntities();
            HostServices();
        }

        private static void InsertDummyEntities()
        {
            InsertCatalogueEntities();
            CreateOperator();
            CreateUser();
        }

        private static void CreateUser()
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                if (lContainer.Users.Where((pUser) => pUser.Name == "Customer").Count() > 0)
                {
                    return;
                }
            }


            User lCustomer = new User()
            {
                Name = "Customer",
                LoginCredential = new LoginCredential() { UserName = "Customer", Password = "COMP5348" },
                Email = "David@Sydney.edu.au",
                Address = "1 Central Park",
                BankAccountNumber = 456,
            };

            ServiceLocator.Current.GetInstance<IUserProvider>().CreateUser(lCustomer);
        }

        private static Stock CreateStock(Book pBook, Warehouse pWarehouse, int pQuantity)
        {
            return new Stock()
            {
                Quantity = pQuantity,
                Warehouse = pWarehouse,
                Book = pBook
            };
        }

        private static void InsertCatalogueEntities()
        {
            using (TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                if (lContainer.Books.Count() == 0)
                {
                    Warehouse lNeutralBay = new Warehouse()
                    {
                        Name = "Neutral Bay",
                        Address = "1 Neutral Bay"
                    };

                    Warehouse lTheWarehouse = new Warehouse()
                    {
                        Name = "The Warehouse",
                        Address = "1 The Warehouse"
                    };

                    Warehouse lStorageKing = new Warehouse()
                    {
                        Name = "Storage King",
                        Address = "1 Storage King"
                    };

                    Warehouse lAmazon = new Warehouse()
                    {
                        Name = "Amazon",
                        Address = "1 Amazon St"
                    };

                    Book lGreatExpectations = new Book() // 4 4 3 1 as Stocks
                    {
                        Author = "Jane Austen",
                        Genre = "Fiction",
                        Price = 20.0,
                        Title = "1 Pride and Prejudice",
                    };

                    Book lSoloist = new Book() // 1 5 2 4 
                    {
                        Author = "Charles Dickens",
                        Genre = "Fiction",
                        Price = 15.0,
                        Title = "2 Grape Expectations"
                    };

                    Book lNoBook = new Book() // 1 5 2 4 
                    {
                        Author = "No Book",
                        Genre = "Fiction",
                        Price = 10.0,
                        Title = "3 No Book"
                    };

                    Stock lGreatExpectationNeutralBayStock = CreateStock(lGreatExpectations, lNeutralBay, 1);
                    Stock lGreatExpectationTheWarehouseStock = CreateStock(lGreatExpectations, lTheWarehouse, 0);
                    Stock lGreatExpectationStorageKingStock = CreateStock(lGreatExpectations, lStorageKing, 0);
                    Stock lGreatExpectationAmazonStock = CreateStock(lGreatExpectations, lAmazon, 1);

                    Stock lSoloistNeutralBayStock = CreateStock(lSoloist, lNeutralBay, 0);
                    Stock lSoloistTheWarehouseStock = CreateStock(lSoloist, lTheWarehouse, 1);
                    Stock lSoloistStorageKingStock = CreateStock(lSoloist, lStorageKing, 0);
                    Stock lSoloistAmazonStock = CreateStock(lSoloist, lAmazon, 0);


                    Stock lNoBookNeutralBayStock = CreateStock(lNoBook, lNeutralBay, 0);
                    Stock lNoBookTheWarehouseStock = CreateStock(lNoBook, lTheWarehouse, 0);
                    Stock lNoBookStorageKingStock = CreateStock(lNoBook, lStorageKing, 1);
                    Stock lNoBookAmazonStock = CreateStock(lNoBook, lAmazon, 1);

                    lContainer.Books.Add(lGreatExpectations);
                    lContainer.Books.Add(lSoloist);
                    lContainer.Books.Add(lNoBook);

                    lContainer.Stocks.Add(lGreatExpectationNeutralBayStock);
                    lContainer.Stocks.Add(lGreatExpectationTheWarehouseStock);
                    lContainer.Stocks.Add(lGreatExpectationStorageKingStock);
                    lContainer.Stocks.Add(lGreatExpectationAmazonStock);

                    lContainer.Stocks.Add(lSoloistNeutralBayStock);
                    lContainer.Stocks.Add(lSoloistStorageKingStock);
                    lContainer.Stocks.Add(lSoloistTheWarehouseStock);
                    lContainer.Stocks.Add(lSoloistAmazonStock);

                    lContainer.Stocks.Add(lNoBookNeutralBayStock);
                    lContainer.Stocks.Add(lNoBookTheWarehouseStock);
                    lContainer.Stocks.Add(lNoBookStorageKingStock);
                    lContainer.Stocks.Add(lNoBookAmazonStock);

                    //for (int i = 1; i < 10; i++)
                    //{
                    //    Book lItem = new Book()
                    //    {
                    //        Author = String.Format("Author {0}", i.ToString()),
                    //        Genre = String.Format("Genre {0}", i),
                    //        Price = i,
                    //        Title = String.Format("Title {0}", i)
                    //    };

                    //    lContainer.Stocks.Add(lSoloistStock);

                    //    Stock lStock = new Stock()
                    //    {
                    //        Book = lItem,
                    //        Quantity = 10 + i,
                    //        // Warehouse = String.Format("Warehouse {0}", i)
                    //        Warehouse = new Warehouse()
                    //        {
                    //            Name = String.Format("Warehouse {0}", i)
                    //        }
                    //    };

                    //    lContainer.Stocks.Add(lStock);
                    //}

                    lContainer.SaveChanges();
                    lScope.Complete();
                }
            }
        }



        private static void CreateOperator()
        {
            Role lOperatorRole = new Role() { Name = "Operator" };
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                if (lContainer.Roles.Count() > 0)
                {
                    return;
                }
            }
            User lOperator = new User()
            {
                Name = "Operator",
                LoginCredential = new LoginCredential() { UserName = "Operator", Password = "COMP5348" },
                Email = "Wang@Sydney.edu.au",
                Address = "1 Central Park"
            };

            lOperator.Roles.Add(lOperatorRole);

            ServiceLocator.Current.GetInstance<IUserProvider>().CreateUser(lOperator);
        }

        private static void ResolveDependencies()
        {

            UnityContainer lContainer = new UnityContainer();
            UnityConfigurationSection lSection
                    = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
            lSection.Containers["containerOne"].Configure(lContainer);
            UnityServiceLocator locator = new UnityServiceLocator(lContainer);
            ServiceLocator.SetLocatorProvider(() => locator);
        }


        private static void HostServices()
        {
            List<ServiceHost> lHosts = new List<ServiceHost>();
            try
            {

                Configuration lAppConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ServiceModelSectionGroup lServiceModel = ServiceModelSectionGroup.GetSectionGroup(lAppConfig);

                System.ServiceModel.Configuration.ServicesSection lServices = lServiceModel.Services;
                foreach (ServiceElement lServiceElement in lServices.Services)
                {
                    ServiceHost lHost = new ServiceHost(Type.GetType(GetAssemblyQualifiedServiceName(lServiceElement.Name)));
                    lHost.Open();
                    lHosts.Add(lHost);
                }
                Console.WriteLine("BookStore Service Started, press Q key to quit");
                while (Console.ReadKey().Key != ConsoleKey.Q);
            }
            finally
            {
                foreach (ServiceHost lHost in lHosts)
                {
                    lHost.Close();
                }
            }
        }

        private static String GetAssemblyQualifiedServiceName(String pServiceName)
        {
            return String.Format("{0}, {1}", pServiceName, System.Configuration.ConfigurationManager.AppSettings["ServiceAssemblyName"].ToString());
        }
    }
}
