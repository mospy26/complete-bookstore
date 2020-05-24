using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bank.Business.Components.Interfaces;
using Bank.Business.Entities;
using System.Transactions;
using System.Data;
using System.Data.Entity.Infrastructure;
using Bank.Services.Interfaces;

namespace Bank.Business.Components
{
    public class TransferProvider : ITransferProvider
    {

        public string Transfer(double pAmount, int pFromAcctNumber, int pToAcctNumber)
        {
            using (TransactionScope lScope = new TransactionScope())
            using (BankEntityModelContainer lContainer = new BankEntityModelContainer())
            {
                try
                {
                    // find the two account entities and add them to the Container
                    Account lFromAcct = lContainer.Accounts.Where(account => pFromAcctNumber == account.AccountNumber).First(); 
                    Account lToAcct = lContainer.Accounts.Where(account => pToAcctNumber == account.AccountNumber).First();
                    if (lFromAcct.Balance - pAmount < 0)
                    {
                        String lMessage = "Transfer Failed: Insufficient Amount";
                        Console.WriteLine("=================Transfer=================");
                        Console.WriteLine("From: " + pFromAcctNumber);
                        Console.WriteLine("To: " + pToAcctNumber);
                        Console.WriteLine("Total: " + pAmount);
                        Console.WriteLine("Transfer time: " + DateTime.Now);
                        Console.WriteLine("Status: TRANSFER FAILED");
                        Console.WriteLine("==========================================" + "\n");
                        return lMessage;
                    }

                    // update the two accounts
                    lFromAcct.Withdraw(pAmount);
                    lToAcct.Deposit(pAmount);

                    Console.WriteLine("=================Transfer=================");
                    Console.WriteLine("From: " + pFromAcctNumber);
                    Console.WriteLine("To: " + pToAcctNumber);
                    Console.WriteLine("Total: " + pAmount);
                    Console.WriteLine("Transfer time: " + DateTime.Now);
                    Console.WriteLine("Status: TRANSFER COMPLETED");
                    Console.WriteLine("==========================================" + "\n");

                    // save changed entities and finish the transaction
                    lContainer.SaveChanges();
                    lScope.Complete();
                }
                catch (ArgumentNullException lException)
                {
                    Console.WriteLine("===========Error in Transfer Money============");
                    Console.WriteLine("Error occured while transferring money:  " + lException.Message);
                    Console.WriteLine("Error: one of the accounts does not exist");
                    Console.WriteLine("==============================================");
                    Console.WriteLine(" ");
                    String lMessage = "Transfer Failed: One of the accounts does not exist"; // TODO Find the proper account
                    Console.WriteLine(lMessage);
                    return lMessage;
                }
                catch (Exception lException)
                {
                    Console.WriteLine("===========Error in Transfer Money============");
                    Console.WriteLine("Error occured while transferring money:  " + lException.Message);
                    Console.WriteLine("Error: Unknown Error");
                    Console.WriteLine("==============================================");
                    Console.WriteLine(" ");
                    return "Transfer Failed: Unknown Error";
                }
            }
            string lSuccessMessage = "Transfer Success";
            Console.WriteLine(lSuccessMessage);
            return lSuccessMessage;
        }

        private Account GetAccountFromNumber(int pToAcctNumber)
        {
            using (BankEntityModelContainer lContainer = new BankEntityModelContainer())
            {
                Console.WriteLine("===============Get Account Numbe===============");
                Console.WriteLine("Account number gotten: " + pToAcctNumber);
                Console.WriteLine("Transfer time: " + DateTime.Now);
                Console.WriteLine("===============================================");
                Console.WriteLine(" ");
                return lContainer.Accounts.Where((pAcct) => (pAcct.AccountNumber == pToAcctNumber)).FirstOrDefault();
            }
        }
    }
}
