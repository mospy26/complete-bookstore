using System;
using DeliveryCo.Services.Interfaces;
using System.ServiceModel;
using BookStore.Services.Interfaces;

namespace DeliveryCo.Business.Components
{
    public class ExternalServiceFactory
    {
        private static ExternalServiceFactory sFactory = new ExternalServiceFactory();
        public static ExternalServiceFactory Instance
        {
            get
            {
                return sFactory;
            }
        }

        public IOrderService OrderService
        {
            get
            {
                return GetTcpService<IOrderService>("net.tcp://localhost:9010/OrderService");
            }
        }

        private T GetTcpService<T>(String pAddress)
        {
            NetTcpBinding tcpBinding = new NetTcpBinding() { TransactionFlow = true };
            EndpointAddress address = new EndpointAddress(pAddress);
            return new ChannelFactory<T>(tcpBinding, pAddress).CreateChannel();
        }
    }
}
