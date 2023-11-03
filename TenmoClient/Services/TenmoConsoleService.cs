using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using TenmoClient.Models;
using TenmoServer.Models;

namespace TenmoClient.Services
{
    public class TenmoConsoleService : ConsoleService
    {
        /************************************************************
            Print methods
        ************************************************************/
        public void PrintLoginMenu()
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine("Welcome to TEnmo!");
            Console.WriteLine("1: Login");
            Console.WriteLine("2: Register");
            Console.WriteLine("0: Exit");
            Console.WriteLine("---------");
        }

        public void PrintMainMenu(string username)
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine($"Hello, {username}!");
            Console.WriteLine("1: View your current balance");
            Console.WriteLine("2: View your past transfers");
            Console.WriteLine("3: View your pending requests");
            Console.WriteLine("4: Send TE bucks");
            Console.WriteLine("5: Request TE bucks");
            Console.WriteLine("6: Log out");
            Console.WriteLine("0: Exit");
            Console.WriteLine("---------");
        }

        public TenmoClient.Models.LoginUser PromptForLogin()
        {
            string username = PromptForString("User name");
            if (String.IsNullOrWhiteSpace(username))
            {
                return null;
            }
            string password = PromptForHiddenString("Password");

            TenmoClient.Models.LoginUser loginUser = new TenmoClient.Models.LoginUser
            {
                Username = username,
                Password = password
            };
            return loginUser;
        }


        // Add application-specific UI methods here...
        public void PrintAccountBalance(decimal balance)
        {
            Console.WriteLine($"Your current account balance is : {balance}");
        }

        public void PrintPastTransactions(List<Transfer> transfers, TenmoApiService tenmoApiService)
        {
            if (transfers.Count == 0)
            {
                Console.WriteLine("No previous transfers.");
            }
            else
            {
                Console.WriteLine("-------------------------------------------");
                Console.WriteLine("Transfers");
                Console.WriteLine("ID          From/To                 Amount");
                Console.WriteLine("-------------------------------------------");
                foreach (Transfer transfer in transfers)
                {
                    if (transfer.AccountToId == tenmoApiService.UserId)
                    {
                        Console.WriteLine($"{transfer.TransferId}          From: {transfer.AccountFromName}                 $ {transfer.TransactionAmount}");
                    }
                    else if (transfer.AccountFromId == tenmoApiService.UserId)
                    {
                        Console.WriteLine($"{transfer.TransferId}          To: {transfer.AccountToName}                 $ {transfer.TransactionAmount}");
                    }
                }
                Console.WriteLine("---------");
            }
        }
        public void PrintTransactionDetails(int menuSelection, List<Transfer> transfers)
        {
            foreach (Transfer transfer in transfers)
            {
                
                if (menuSelection == transfer.TransferId)
                {
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("Transfer Details");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine($"Id: {transfer.TransferId}");
                    Console.WriteLine($"From: {transfer.AccountFromName}");
                    Console.WriteLine($"To: {transfer.AccountToName}");
                    Console.WriteLine($"Type: {transfer.TransferType}");
                    Console.WriteLine($"Status: {transfer.TransferStatus}");
                    Console.WriteLine($"Amount: {transfer.TransactionAmount}");
                }
            }
        }

        public void PrintPendingRequests(List<Transfer> pendingTransfers)
        {
            if (pendingTransfers.Count == 0)
            {
                Console.WriteLine("No previous transfers.");
            }
            else
            {
                Console.WriteLine("-------------------------------------------");
                Console.WriteLine("Pending Transfers");
                Console.WriteLine("ID          To                     Amount");
                Console.WriteLine("-------------------------------------------");
                foreach (Transfer transfer in pendingTransfers)
                {
                    if (transfer.TransferStatus == 0)
                    {
                        Console.WriteLine($"{transfer.TransferId}          {transfer.AccountToName}                     $ {transfer.TransactionAmount}");
                    }
                }
                Console.WriteLine("---------");
            }
        }

        public void PrintPendingTransferMenu()
        {
            Console.WriteLine("1: Approve");
            Console.WriteLine("2: Reject");
            Console.WriteLine("0: Don't approve or reject");
            Console.WriteLine("---------");
        }
        public void UpdateTransferStatus(int menuSelection, Transfer transfer, TenmoApiService tenmoApiService)
        {
            if (menuSelection == 0)
            {
                // Don't approve or reject
                transfer.TransferStatus = 1;
                tenmoApiService.UpdateTransfer(transfer);
            }
            else if (menuSelection == 1)
            {
                // Approve
                transfer.TransferStatus = 2;
                tenmoApiService.UpdateTransfer(transfer);
            }
            else if (menuSelection == 2)
            {
                //Reject
                transfer.TransferStatus = 3;
                tenmoApiService.UpdateTransfer(transfer);
            }
        } 

        public void PrintUserList(List<User> userList, TenmoApiService tenmoApiService)
        {
            Console.WriteLine($"|-------------- Users --------------|");
            Console.WriteLine($"|    Id | {tenmoApiService.UserId}                  |");
            Console.WriteLine($"|-------+---------------------------|");
            foreach (User user in userList)
            {
                if (user.UserId != tenmoApiService.UserId)
                {
                    Console.WriteLine($" {user.UserId} | {user.Username}");
                }
            }
            Console.WriteLine($"|-----------------------------------|");
        }

        public void SendTEBucks()
        {
            
        }
    }
}
