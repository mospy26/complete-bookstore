﻿using DeliveryCo.Services;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Practices.Unity.ServiceLocatorAdapter;
using System;
using System.Configuration;
using System.ServiceModel;

namespace DeliveryCo.Process
{
    class Program
    {
        static void Main(string[] args)
        {
            ResolveDependencies();
            using (ServiceHost lHost = new ServiceHost(typeof(DeliveryService)))
            {
                lHost.Open();
                Console.WriteLine("Delivery Service started. Press Q to quit");
                while (Console.ReadKey().Key != ConsoleKey.Q);
            }

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
    }
}
