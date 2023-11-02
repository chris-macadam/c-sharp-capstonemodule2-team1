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
                console.PrintAccountBalance(balance);
                console.Pause();
            }

            if (menuSelection == 2)
            {
                // View your past transfers
                List<Transfer> transfers = tenmoApiService.GetTransfers();
                console.PrintPastTransactions(transfers, tenmoApiService);                
                menuSelection = console.PromptForInteger($"Please enter transfer ID to view details (0 to cancel)", 0, int.MaxValue);
                if (menuSelection == 0)
                {
                    console.PrintMainMenu(tenmoApiService.Username);
                }
                else
                {
                    console.PrintTransactionDetails(menuSelection, transfers);
                }                
            }

            if (menuSelection == 3)
            {
                // View your pending requests
                List<Transfer> pendingTransfers = tenmoApiService.GetPendingTransfers();
                console.PrintPendingRequests(pendingTransfers);
                menuSelection = console.PromptForInteger($"Please enter transferID to approve/reject (0 to cancel)", 0, int.MaxValue);
                foreach (Transfer transfer in pendingTransfers)
                {
                    if (menuSelection == 0)
                    {
                        console.PrintMainMenu(tenmoApiService.Username);
                    }
                    if (menuSelection == transfer.TransferId)
                    {
                        console.PrintPendingTransferMenu();
                        menuSelection = console.PromptForInteger($"Please choose an option", 0, 2);
                        console.UpdateTransferStatus(menuSelection, transfer, tenmoApiService);
                    }
                }
            }

            if (menuSelection == 4)
            {
                // Send TE bucks

                Console.WriteLine($"|-------------- Users --------------|");
                Console.WriteLine($"|    Id | {tenmoApiService.UserId}                  |");
                Console.WriteLine($"|-------+---------------------------|");
                List<User> users =tenmoApiService.GetUsers();
                foreach (User user in users)
                {
                Console.WriteLine($"{tenmoApiService.UserId} | {tenmoApiService.Username}");
                }
                Console.WriteLine($"|-----------------------------------|");

                string recipientUsername = console.PromptForString("Enter the recipient's username: ");
                decimal amount = console.PromptForDecimal("Enter the amount to send: ");

                Console.WriteLine($"You are about to send $ {amount} to {recipientUsername}.");
                string confirm = console.PromptForString($"Confirm the transfer (yes/no): ");
                if (confirm.Contains("yes", StringComparison.OrdinalIgnoreCase))
                {
                    Transfer transfer = new Transfer();
                    tenmoApiService.SendTransfer(transfer, tenmoApiService.UserId);
                }
                else
                {
                    console.PrintMainMenu(tenmoApiService.Username);
                }


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
