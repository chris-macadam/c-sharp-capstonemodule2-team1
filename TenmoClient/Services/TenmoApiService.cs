﻿using RestSharp;
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

        public decimal GetAccountBalance(int userId)
        {
            RestRequest request = new RestRequest($"users/{UserId}/balance");
            IRestResponse<decimal> response = client.Get<decimal>(request);
            CheckForError(response);
            return response.Data;
        }

        public Transfer SendTransfer(Transfer transaction)
        {
            RestRequest request = new RestRequest("transactions");
            request.AddJsonBody(transaction);
            IRestResponse<Transfer> response = client.Post<Transfer>(request);
            CheckForError(response);
            return response.Data;
        }

        public List<Transfer> GetTransfers()
        {
            RestRequest request = new RestRequest("transactions");
            IRestResponse<List<Transfer>> response = client.Get<List<Transfer>>(request);
            CheckForError(response);
            return response.Data;
        }

        public Transfer GetTransferByID(int id)
        {
            RestRequest request = new RestRequest($"transactions/{id}");
            IRestResponse<Transfer> response = client.Get<Transfer>(request);
            CheckForError(response);
            return response.Data;
        }

        public List<Transfer> GetPendingTransers()
        {
            RestRequest request = new RestRequest("transactions/pending");
            IRestResponse<List<Transfer>> response = client.Get<List<Transfer>>(request);
            CheckForError(response);
            return response.Data;
        }

        public Transfer SetTransferStatus(Transfer transfer)
        {
            RestRequest request = new RestRequest($"transactions/{transfer.Id}");
            request.AddJsonBody(transfer);
            IRestResponse<Transfer> response = client.Put<Transfer>(request);
            CheckForError(response);
            return response.Data;
        }
    }
}
