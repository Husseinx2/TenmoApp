
﻿using RestSharp;
using System.Collections.Generic;
using TenmoClient.Models;

namespace TenmoClient.Services
{
    public class TenmoApiService : AuthenticatedApiService
    {
        public readonly string ApiUrl;

        public TenmoApiService(string apiUrl) : base(apiUrl) { }

        // Add methods to call api here...
        public decimal GetBalance()
        {
            RestRequest request = new RestRequest($"account/{UserId}/balance");
            IRestResponse<decimal> response = client.Get<decimal>(request);
            CheckForError(response);
            return response.Data;
        }

        public IList<ApiUser> Users()
        {
            RestRequest request = new RestRequest($"users");
            IRestResponse<IList<ApiUser>> response = client.Get<IList<ApiUser>>(request);
            CheckForError(response);
            return response.Data;
        }


        public int GetAccountId(int userId)
        {
            RestRequest request = new RestRequest($"account/{userId}");
            IRestResponse<int> response = client.Get<int>(request);
            CheckForError(response);
            return response.Data;
        }

        public bool Send(int recipientUserId, decimal amount)
        {
            bool result = false;
           
                Transfer transfer = new Transfer();
                int senderUserId = UserId;
                transfer.AccountFrom = GetAccountId(senderUserId);
                transfer.AccountTo = GetAccountId(recipientUserId);
                transfer.TransferTypeId = 2;
                transfer.TransferStatusId = 2;
                transfer.Amount = amount;

                RestRequest request = new RestRequest($"transfer");
                request.AddJsonBody(transfer);
                IRestResponse<Transfer> response = client.Post<Transfer>(request);
                CheckForError(response);
                result = response.IsSuccessful;
                return result;
        }
        public string GetUserName(int accountId)
        {
            RestRequest request = new RestRequest($"users/username/{accountId}");
            IRestResponse<string> response = client.Get<string>(request);
            CheckForError(response);
            return response.Data;
        }
        public List<Transfer> GetTransfers()
        {
            int accountId = GetAccountId(UserId);

            RestRequest request = new RestRequest($"transfer/{accountId}");
            IRestResponse<List<Transfer>> response = client.Get<List<Transfer>>(request);
            CheckForError(response);
            return response.Data;
        }

        public string GetTransferStatus(int transferId)
        {
            RestRequest request = new RestRequest($"transfer/transferstatus/{transferId}");
            IRestResponse<string> response = client.Get<string>(request);
            CheckForError(response);
            return response.Data;
        }

        public string GetTransferType(int transferId)
        {
            RestRequest request = new RestRequest($"transfer/transfertype/{transferId}");
            IRestResponse<string> response = client.Get<string>(request);
            CheckForError(response);
            return response.Data;
        }
    }
}
