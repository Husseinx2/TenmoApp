using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Channels;
using TenmoClient.Models;
using TenmoClient.Services;

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
                GetBalance();
            }

            if (menuSelection == 2)
            {
                GetTransfers();
            }

            if (menuSelection == 3)
            {
                // View your pending requests
            }

            if (menuSelection == 4)
            {
                ListUsers();

                int recipientId = console.PromptForInteger("Id of the user you are sending to[0]");
                decimal amount = console.PromptForDecimal("Enter amount to send");

                SendMoney(recipientId, amount);

                console.Pause();
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
            LoginUser loginUser = console.PromptForLogin();
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
            LoginUser registerUser = console.PromptForLogin();
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
        public void GetBalance()
        {
            Console.WriteLine($"Your current account balance is: {tenmoApiService.GetBalance():C2}");
            console.Pause();
        }
        public void ListUsers()
        {
            string headerLine = "-------------- Users --------------";
            string sectionLine = "-------+---------------------------";
            string endLine = "-----------------------------------";

            const int Width1 = 6;
            const int Width2 = 26;
            string cat1 = "Id";
            string cat2 = "Username";
            Console.WriteLine(console.Border(headerLine));
            Console.WriteLine(console.Border($"{cat1,Width1} | {cat2,-Width2}"));
            Console.WriteLine(console.Border(sectionLine));


            foreach (ApiUser user in tenmoApiService.Users())
            {
                if (user.UserId != tenmoApiService.UserId)
                {
                    Console.WriteLine(console.Border($"{user.UserId,Width1} | {user.Username,-Width2}"));
                }
            }
            Console.WriteLine(console.Border(endLine));

        }

        private void SendMoney(int recipientId, decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Enter a valid Amount.");
            }
            else if (amount > tenmoApiService.GetBalance())
            {
                Console.WriteLine("Insufficient Funds");
            }
            else if (recipientId == tenmoApiService.UserId)
            {
                Console.WriteLine("Enter a Valid User");
            }
            else if (tenmoApiService.Send(recipientId, amount))
            {
                Console.WriteLine("Transfer successful.");
            }
        }
        private void GetTransfers()
        {
            List<Transfer> transfers = tenmoApiService.GetTransfers();
            if (transfers == null)
            {

                Console.WriteLine("No Transfers Available");
                console.Pause();
                return;
            }

            const int IdWidth = 12;
            const int FromTo = 25;
            string line = "-------------------------------------------";
            Console.WriteLine(line);
            Console.WriteLine("Transfers");
            Console.WriteLine("ID".PadRight(IdWidth) + "From/To".PadRight(FromTo) + "Amount");
            Console.WriteLine(line);
            try
            {
                foreach (Transfer transfer in transfers)
                {
                    int accountId = tenmoApiService.GetAccountId(tenmoApiService.UserId);
                    string fromToDisplay = accountId == transfer.AccountTo ? $"From: {tenmoApiService.GetUserName(transfer.AccountFrom)} " : $"To: {tenmoApiService.GetUserName(transfer.AccountTo)} ";
                    Console.WriteLine($"{transfer.TransferId,-IdWidth}{fromToDisplay,-FromTo}{transfer.Amount:c2}");
                }


                int result = console.PromptForInteger("Select transfer Id [0 to cancel]");

                if (result != 0)
                {
                    foreach (Transfer transfer in transfers)
                    {
                        if (transfer.TransferId == result)
                        {

                            Console.WriteLine(line);
                            Console.WriteLine("Transfer Details");
                            Console.WriteLine(line);
                            Console.WriteLine($"Id: {transfer.TransferId}");
                            Console.WriteLine($"From: {tenmoApiService.GetUserName(transfer.AccountFrom)}");
                            Console.WriteLine($"To: {tenmoApiService.GetUserName(transfer.AccountTo)}");
                            Console.WriteLine($"Type: {tenmoApiService.GetTransferType(transfer.TransferTypeId)}");
                            Console.WriteLine($"Status: {tenmoApiService.GetTransferStatus(transfer.TransferStatusId)}");
                            Console.WriteLine($"Amount: {transfer.Amount:C2}");
                            break;
                        }
                    }

                }
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("There was a problem reaching the Server");
            }

            console.Pause();
        }


    }
}