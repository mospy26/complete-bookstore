using System.ServiceModel;

namespace Bank.Services.Interfaces
{
    [ServiceContract]
    public interface ITransferService
    {
        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        string Transfer(double pAmount, int pFromAcctNumber, int pToAcctNumber);
    }
}
