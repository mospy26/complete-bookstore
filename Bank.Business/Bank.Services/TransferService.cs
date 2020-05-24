using Bank.Business.Components.Interfaces;
using Bank.Services.Interfaces;
using Microsoft.Practices.ServiceLocation;
using System.ServiceModel;

namespace Bank.Services
{
    public class TransferService : ITransferService
    {
        private ITransferProvider TransferProvider
        {
            get { return ServiceLocator.Current.GetInstance<ITransferProvider>(); }
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public string Transfer(double pAmount, int pFromAcctNumber, int pToAcctNumber)
        {
            return TransferProvider.Transfer(pAmount, pFromAcctNumber, pToAcctNumber);
        }
    }
}
