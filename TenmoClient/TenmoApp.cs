using System;
using System.Collections.Generic;
using TenmoClient.Models;
using TenmoClient.Services;
using TenmoServer.Models;

namespace TenmoClient
{
    public class TenmoApp
    {
        private readonly TenmoConsoleService console = new TenmoConsoleService();
        private readonly TenmoApiService tenmoApiService;

        public TenmoApp(string apiUrl)
        {
            tenmoApiService = new TenmoApiService(apiUrl);
        }

        public void Run()
        {
            bool keepGoing = true;
            while (keepGoing)
            {
                // The menu changes depending on whether the user is logged in or not
                if (tenmoApiService.IsLoggedIn)
                {
                    keepGoing = RunAuthenticated();
                }
                else // User is not yet logged in
                {
                    keepGoing = RunUnauthenticated();
                }
            }
        }

        private bool RunUnauthenticated()
        {
            console.PrintLoginMenu();
            int menuSelection = console.PromptForInteger("Please choose an option", 0, 2, 1);
            while (true)
            {
                if (menuSelection == 0)
                {
                    return false;   // Exit the main menu loop
                }

                if (menuSelection == 1)
                {
                    // Log in
                    Login();
                    return true;    // Keep the main menu loop going
                }

                if (menuSelection == 2)
                {
                    // Register a new user
                    Register();
                    return true;    // Keep the main menu loop going
                }
                console.PrintError("Invalid selection. Please choose an option.");
                console.Pause();
            }
        }

        private bool RunAuthenticated()
        {
            console.PrintMainMenu(tenmoApiService.Username);
            int menuSelection = console.PromptForInteger("Please choose an option", 0, 6);
            if (menuSelection == 0)
            {
                // Exit the loop
                return false;
            }

            if (menuSelection == 1)
            {
                // View your current balance
                decimal balance = tenmoApiService.GetAccountBalance(tenmoApiService.UserId);
                Console.WriteLine($"Your current account balance is : {balance}");
            }

            if (menuSelection == 2)
            {
                // View your past transfers
                List<Transfer> transfers = tenmoApiService.GetTransfers();
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
                    menuSelection = console.PromptForInteger($"Please enter transfer ID to view details (0 to cancel): ", 0, int.MaxValue);
                    foreach (Transfer transfer in transfers)
                    {
                        if (menuSelection == 0)
                        {
                            return false;
                        }
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
            }

            if (menuSelection == 3)
            {
                // View your pending requests
                List<Transfer> pendingTransfers = tenmoApiService.GetPendingTransers();
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
                    menuSelection = console.PromptForInteger($"Please enter transferID to approve/reject (0 to cancel): ", 0, int.MaxValue);
                    foreach (Transfer transfer in pendingTransfers)
                    {
                        if (menuSelection == 0)
                        {
                            return false;
                        }
                        if (menuSelection == transfer.TransferId)
                        {
                            Console.WriteLine("1: Approve");
                            Console.WriteLine("2: Reject");
                            Console.WriteLine("3: Don't approve or reject");
                            Console.WriteLine("---------");
                            menuSelection = console.PromptForInteger($"Please choose an option: ", 0, 2);
                            if (menuSelection == 0)
                            {
                                // Don't approve or reject
                                transfer.TransferStatus = 0;
                                tenmoApiService.SetTransferStatus(transfer);
                            }
                            else if (menuSelection == 1)
                            {
                                // Approve
                                transfer.TransferStatus = 1;
                                tenmoApiService.SetTransferStatus(transfer);
                            }
                            else if (menuSelection == 2)
                            {
                                //Reject
                                transfer.TransferStatus = 2;
                                tenmoApiService.SetTransferStatus(transfer);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (menuSelection == 4)
            {
                // Send TE bucks
                 
            }

            if (menuSelection == 5)
            {
                // Request TE bucks
            }

            if (menuSelection == 6)
            {
                // Log out
                tenmoApiService.Logout();
                console.PrintSuccess("You are now logged out");
            }

            return true;    // Keep the main menu loop going
        }

        private void Login()
        {
            TenmoClient.Models.LoginUser loginUser = console.PromptForLogin();
            if (loginUser == null)
            {
                return;
            }

            try
            {
                ApiUser user = tenmoApiService.Login(loginUser);
                if (user == null)
                {
                    console.PrintError("Login failed.");
                }
                else
                {
                    console.PrintSuccess("You are now logged in");
                }
            }
            catch (Exception)
            {
                console.PrintError("Login failed.");
            }
            console.Pause();
        }

        private void Register()
        {
            TenmoClient.Models.LoginUser registerUser = console.PromptForLogin();
            if (registerUser == null)
            {
                return;
            }
            try
            {
                bool isRegistered = tenmoApiService.Register(registerUser);
                if (isRegistered)
                {
                    console.PrintSuccess("Registration was successful. Please log in.");
                }
                else
                {
                    console.PrintError("Registration was unsuccessful.");
                }
            }
            catch (Exception)
            {
                console.PrintError("Registration was unsuccessful.");
            }
            console.Pause();
        }
    }
}
