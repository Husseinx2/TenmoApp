
using RestSharp;
using System.Collections.Generic;
using System.Net.Http;
using TenmoClient.Models;

namespace TenmoClient.Services
{
    public class TenmoApiService : AuthenticatedApiService
    {
        public readonly string ApiUrl;

        public TenmoApiService(string apiUrl) : base(apiUrl) { }


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
        public string GetUserName(int accountId)
        {
            RestRequest request = new RestRequest($"users/username/{accountId}");
            IRestResponse<string> response = client.Get<string>(request);
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
            bool result;
            try
            {
                Transfer transfer = new Transfer();
                int senderUserId = UserId;
                transfer.AccountFrom = GetAccountId(senderUserId);
                transfer.AccountTo = GetAccountId(recipientUserId);
                transfer.TransferTypeId = 2;
                transfer.TransferStatusId = 2;
                transfer.Amount = amount;

                RestRequest request = new RestRequest($"transfer/send");
                request.AddJsonBody(transfer);
                IRestResponse<Transfer> response = client.Post<Transfer>(request);
                CheckForError(response);
                result = response.IsSuccessful;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            return result;
        }

        public bool Request(int requesteeUserId, decimal amount)
        {
            bool result;
            try
            {
                Transfer transfer = new Transfer();
                int requesterUserId = UserId;
                transfer.AccountFrom = GetAccountId(requesteeUserId);
                transfer.AccountTo = GetAccountId(requesterUserId);
                transfer.TransferTypeId = 1;
                transfer.TransferStatusId = 1;
                transfer.Amount = amount;

                RestRequest request = new RestRequest($"transfer/request");
                request.AddJsonBody(transfer);
                IRestResponse<Transfer> response = client.Post<Transfer>(request);
                CheckForError(response);
                result = response.IsSuccessful;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            return result;
        }

        public List<Transfer> GetTransfers()
        {
            try
            {
                int accountId = GetAccountId(UserId);
                RestRequest request = new RestRequest($"transfer/{accountId}");
                IRestResponse<List<Transfer>> response = client.Get<List<Transfer>>(request);
                CheckForError(response);
                return response.Data;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public string GetTransferStatus(int transferId)
        {
            try
            {
                RestRequest request = new RestRequest($"transfer/transferstatus/{transferId}");
                IRestResponse<string> response = client.Get<string>(request);
                CheckForError(response);
                return response.Data;
            }
            catch (HttpRequestException)
            {
                return "";
            }

        }

        public string GetTransferType(int transferId)
        {
            try
            {
                RestRequest request = new RestRequest($"transfer/transfertype/{transferId}");
                IRestResponse<string> response = client.Get<string>(request);
                CheckForError(response);
                return response.Data;
            }
            catch (HttpRequestException)
            {
                return "";
            }

        }
    }
}
