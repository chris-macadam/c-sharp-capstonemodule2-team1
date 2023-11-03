using RestSharp;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Transactions;
using TenmoServer.Models;

namespace TenmoClient.Services
{
    public class TenmoApiService : AuthenticatedApiService
    {
        public readonly string ApiUrl;

        public TenmoApiService(string apiUrl) : base(apiUrl) { }

        // Add methods to call api here...

        public List<User> GetUsers()
        {
            RestRequest request = new RestRequest("users");
            IRestResponse<List<User>> response = client.Get<List<User>>(request);
            CheckForError(response);
            return response.Data;
        }

        public Account GetAccountFromUserId(int userId) 
        {
            RestRequest request = new RestRequest($"users/{userId}/account");
            IRestResponse<Account> response = client.Get<Account>(request);
            CheckForError(response);
            return response.Data;
        }


        //public decimal GetAccountBalance(int userId)
        //{
        //    RestRequest request = new RestRequest($"users/{userId}/balance");
        //    IRestResponse<decimal> response = client.Get<decimal>(request);
        //    CheckForError(response);
        //    return response.Data;
        //}


        public Transfer SendTransfer(Transfer transaction, int userId)
        {
            RestRequest request = new RestRequest($"transfer/{userId}");
            request.AddJsonBody(transaction);
            IRestResponse<Transfer> response = client.Post<Transfer>(request);
            CheckForError(response);
            return response.Data;
        }

        public List<Transfer> GetTransfers(int userId)
        {
            RestRequest request = new RestRequest($"transfer/all/{userId}");
            IRestResponse<List<Transfer>> response = client.Get<List<Transfer>>(request);
            CheckForError(response);
            return response.Data;
        }

        public Transfer GetTransferByID(int id)
        {
            RestRequest request = new RestRequest($"transfer/{id}");
            IRestResponse<Transfer> response = client.Get<Transfer>(request);
            CheckForError(response);
            return response.Data;
        }

        public List<Transfer> GetPendingTransfers()
        {
            RestRequest request = new RestRequest($"transfer/pending/{UserId}");
            IRestResponse<List<Transfer>> response = client.Get<List<Transfer>>(request);
            CheckForError(response);
            return response.Data;
        }

        public Transfer UpdateTransfer(Transfer transfer)
        {
            RestRequest request = new RestRequest($"transfer/{transfer.TransferId}");
            request.AddJsonBody(transfer);
            IRestResponse<Transfer> response = client.Put<Transfer>(request);
            CheckForError(response);
            return response.Data;
        }
    }
}
