using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
                GetPendingRequests();
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
                ListUsers();

                int requesteeId = console.PromptForInteger("Id of the user you are requesting from[0]");
                decimal amount = console.PromptForDecimal("Enter amount to request");

                RequestMoney(requesteeId, amount);

                console.Pause();
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
            const int LineWidth = 35;
            const int IdWidth = 6;
            const int UsernameWidth = 26;

            string tableLine = new string('-', LineWidth);

            string title = "Users";
            string titleDisplay = $" {title} ";
            int titleSectionWidth = (LineWidth - titleDisplay.Length) / 2;
            string titleLineSection = new string('-', titleSectionWidth);
            string titleLine = $"{titleLineSection}{titleDisplay}{titleLineSection}";

            string headerId = "Id";
            string headerUsername = "Username";
            string header = $"{headerId,IdWidth} | {headerUsername,-UsernameWidth}";
            string headerLine = $"{tableLine.Substring(0, IdWidth + 1)}+{tableLine.Substring(IdWidth + 2)}";
            string endLine = tableLine;

            console.WriteBorderLine(titleLine);
            console.WriteBorderLine(header);
            console.WriteBorderLine(headerLine);

            foreach (ApiUser user in tenmoApiService.Users())
            {
                if (user.UserId != tenmoApiService.UserId)
                {
                    string userDataRow = $"{user.UserId,IdWidth} | {user.Username,-UsernameWidth}";
                    console.WriteBorderLine(userDataRow);
                }
            }
            console.WriteBorderLine(endLine);
        }

        private void SendMoney(int recipientId, decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Enter a valid amount.");
            }
            else if (amount > tenmoApiService.GetBalance())
            {
                Console.WriteLine("Insufficient funds.");
            }
            else if (recipientId == tenmoApiService.UserId)
            {
                Console.WriteLine("Enter a valid user.");
            }
            else if (tenmoApiService.Send(recipientId, amount))
            {
                console.PrintSuccess("Transfer successful.");
            }
            else
            {
                console.PrintError("Request failed.");
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
            const int FromToWidth = 25;
            const int AmountWidth = 6;
            const int LineWidth = IdWidth + FromToWidth + AmountWidth;

            string tableLine = new string('-', LineWidth);
            string title = "Transfers";

            string header = "ID".PadRight(IdWidth) + "From/To".PadRight(FromToWidth) + "Amount ".PadLeft(AmountWidth);

            Console.WriteLine(tableLine);
            Console.WriteLine(title);
            Console.WriteLine(header);
            Console.WriteLine(tableLine);

            try
            {
                foreach (Transfer transfer in transfers)
                {
                    if (transfer.TransferStatusId == 2)
                    {
                        int accountId = tenmoApiService.GetAccountId(tenmoApiService.UserId);

                        // If logged in user is recipient, toDisplay shows sender username
                        // If logged in user is sender, toDisplay shows recipient username
                        string fromToDisplay;
                        if (accountId == transfer.AccountTo)
                        {
                            string senderUsername = tenmoApiService.GetUserName(transfer.AccountFrom);
                            fromToDisplay = $"From: {senderUsername} ";
                        }
                        else
                        {
                            string recipientUsername = tenmoApiService.GetUserName(transfer.AccountTo);
                            fromToDisplay = $"To: {recipientUsername} ";
                        }

                        string transferDisplay = $"{transfer.Amount:c2}";
                        string transferDataRow = $"{transfer.TransferId,-IdWidth}{fromToDisplay,-FromToWidth}{transferDisplay,AmountWidth}";
                        Console.WriteLine(transferDataRow);
                    }
                }
                Console.WriteLine(tableLine);

                int transferIdInput = console.PromptForInteger("Select transfer Id [0 to cancel]");

                if (transferIdInput != 0)
                {
                    Transfer transfer = GetTransferByTransferId(transfers, transferIdInput);
                    if (transfer != null)
                    {
                        Console.WriteLine(tableLine);
                        Console.WriteLine("Transfer Details");
                        Console.WriteLine(tableLine);
                        Console.WriteLine($"Id: {transfer.TransferId}");
                        Console.WriteLine($"From: {tenmoApiService.GetUserName(transfer.AccountFrom)}");
                        Console.WriteLine($"To: {tenmoApiService.GetUserName(transfer.AccountTo)}");
                        Console.WriteLine($"Type: {tenmoApiService.GetTransferType(transfer.TransferTypeId)}");
                        Console.WriteLine($"Status: {tenmoApiService.GetTransferStatus(transfer.TransferStatusId)}");
                        Console.WriteLine($"Amount: {transfer.Amount:C2}");
                    }
                    else
                    {
                        console.PrintError("Invalid Transfer Id");
                    }
                }
            }
            catch (HttpRequestException)
            {
                console.PrintError("There was a problem reaching the server");
            }

            console.Pause();
        }
        private void GetPendingRequests()
        {
            bool isUpdated = false;
            List<Transfer> transfers = tenmoApiService.GetTransfers();

            if (transfers == null)
            {
                Console.WriteLine("No Pending Transfers Available");
                console.Pause();
                return;
            }

            const int IdWidth = 12;
            const int FromToWidth = 25;
            const int AmountWidth = 6;
            const int LineWidth = IdWidth + FromToWidth + AmountWidth;

            string tableLine = new string('-', LineWidth);
            string title = "Pending Transfers";

            string header = "ID".PadRight(IdWidth) + "To".PadRight(FromToWidth) + "Amount ".PadLeft(AmountWidth);

            Console.WriteLine(tableLine);
            Console.WriteLine(title);
            Console.WriteLine(header);
            Console.WriteLine(tableLine);

            try
            {
                foreach (Transfer transfer in transfers)
                {
                    int accountId = tenmoApiService.GetAccountId(tenmoApiService.UserId);
                    if (transfer.TransferStatusId == 1 && transfer.AccountFrom == accountId)
                    {

                        string requesterUsername = tenmoApiService.GetUserName(transfer.AccountTo);
                        string toDisplay = $"To: {requesterUsername}";

                        string transferDisplay = $"{transfer.Amount:c2}";
                        string transferDataRow = $"{transfer.TransferId,-IdWidth}{toDisplay,-FromToWidth}{transferDisplay,AmountWidth}";
                        Console.WriteLine(transferDataRow);
                    }
                }
                Console.WriteLine(tableLine);
                int requestId = console.PromptForInteger("Please enter transfer ID to approve/reject (0 to cancel)");
                if (requestId != 0)
                {
                    Transfer transfer = GetTransferByTransferId(transfers, requestId);
                    if (transfer != null)
                    {
                        Console.WriteLine("1: Approve");
                        Console.WriteLine("2: Reject");
                        Console.WriteLine("0: Don't approve or reject");
                        Console.WriteLine(tableLine);
                        int optionInput = console.PromptForInteger("Please choose an option", 0, 2);
                        if (optionInput != 0)
                        {
                            transfer.TransferStatusId = optionInput + 1;
                            if (transfer.TransferStatusId == 2)
                            {
                                if (transfer.Amount <= 0)
                                {
                                    Console.WriteLine("Enter a valid amount.");
                                }
                                else if (transfer.Amount > tenmoApiService.GetBalance())
                                {
                                    Console.WriteLine("Insufficient funds.");
                                }
                                else if (transfer.AccountTo == tenmoApiService.UserId)
                                {
                                    Console.WriteLine("Invalid user.");
                                }
                                else
                                {
                                    isUpdated = tenmoApiService.UpdateRequest(transfer);
                                }
                            }
                            else
                            {
                                isUpdated = tenmoApiService.UpdateRequest(transfer);
                            }
                        }
                        if (isUpdated)
                        {
                            console.PrintSuccess("Request updated.");
                        }
                        else
                        {
                            console.PrintError("Request not updated.");
                        }
                    }
                    else
                    {
                        console.PrintError("Invalid Transfer Id");
                    }
                }
            }
            catch (HttpRequestException)
            {
                console.PrintError("There was a problem reaching the server");
            }
            console.Pause();
        }

        private void RequestMoney(int requesteeId, decimal amount)
        {
            if (amount <= 0)
            {
                console.PrintError("Enter a valid amount.");
            }
            else if (requesteeId == tenmoApiService.UserId)
            {
                console.PrintError("Enter a valid user");
            }
            else if (tenmoApiService.Request(requesteeId, amount))
            {
                console.PrintSuccess("Request sent.");
            }
            else
            {
                console.PrintError("Request failed.");
            }
        }

        public Transfer GetTransferByTransferId(List<Transfer> transfers, int transferId)
        {
            foreach (Transfer transfer in transfers)
            {
                if (transfer.TransferId == transferId)
                {
                    return transfer;
                }
            }

            return null;
        }
    }
}