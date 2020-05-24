namespace Bank.Business.Components.Interfaces
{
    public interface ITransferProvider
    {
        string Transfer(double pAmount, int pFromAcctNumber, int pToAcctNumber);
    }
}
