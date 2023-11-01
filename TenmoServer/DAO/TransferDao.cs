using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Transactions;
using TenmoServer.Exceptions;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public class TransferDao : ITransferDao
    {
        private readonly string connectionString;

        public TransferDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        /// <summary>
        /// Searches for a transfer by transfer ID.
        /// </summary>
        /// <param name="transferId">The identification number of the transaction being searched for</param>
        /// <returns>Returns a Transfer object</returns>
        /// <exception cref="DaoException"></exception>
        public Transfer GetTransferById(int transferId)
        {
            Transfer transfer = new();

            string query = "SELECT transfer_id, transfer_type_id, transfer_status_id, account_from, account_to, amount " +
                            "FROM transfer " +
                            "WHERE transfer_id = @transID;";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@transID", transferId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        transfer = MapRowToTransfer(reader);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }
            return transfer;
        }

        /// <summary>
        /// Searches for all transfers coming from or going to a specific account.
        /// </summary>
        /// <param name="id">The account identification number being searched for.</param>
        /// <returns>A List of Transfer objects.</returns>
        /// <exception cref="DaoException"></exception>
        public IList<Transfer> GetAllTransfers(int id)
        {
            IList<Transfer> transferList = new List<Transfer>();

            string query = "SELECT transfer_id, transfer_status_id, transfer_type_id, account_from, account_to, amount " +
                            "FROM transfer " +
                            "JOIN account ON transfer.account_from = account.user_id AND transfer.account_to = account.user_id " +
                            "JOIN tenmo_user ON account.user_id = tenmo_user.user_id " +
                            "WHERE account_from = @id OR account_to = @id;";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Transfer transfer = MapRowToTransfer(reader);
                        transferList.Add(transfer);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }
            return transferList;
        }

        /// <summary>
        /// Creates a pending transaction between two accounts.
        /// </summary>
        /// <param name="transferType">If the transfer is being created by the sender or reciever. 1 to request funds, 2 for sending funds</param>
        /// <param name="accountFrom">The account identification number the funds are being sent from.</param>
        /// <param name="accountTo">The account identification number the funds are being sent to.</param>
        /// <param name="amount">The amount that is being sent.</param>
        /// <returns>Returns a Transfer object with the transaction details.</returns>
        /// <exception cref="DaoException"></exception>
        public Transfer CreateTransfer(int transferType, int accountFrom, int accountTo, decimal amount)
        {
            Transfer newTransaction = null;

            string query = "INSERT INTO transfer (transfer_type_id, transfer_status_id, account_from, account_to, amount) " +
                            "OUTPUT inserted.transfer_id " +
                            "VALUES (@transferType, @transferStatus, @accountFrom, @accountTo, @amount)";

            int newTranferId = 0;
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@transferType", transferType);
                    cmd.Parameters.AddWithValue("@transferStatus", 1);
                    cmd.Parameters.AddWithValue("@accountFrom", accountFrom);
                    cmd.Parameters.AddWithValue("@accountTo", accountTo);
                    cmd.Parameters.AddWithValue("@AMOUNT", amount);

                    newTranferId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                newTransaction = GetTransferById(newTranferId);
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }
            return newTransaction;
        }

        /// <summary>
        /// Returns a list of all pending transfers associated with a provided account id.
        /// </summary>
        /// <param name="id">Account identification number for the active user.</param>
        /// <returns>Returns a list of Transfer objects.</returns>
        /// <exception cref="DaoException"></exception>
        public IList<Transfer> GetPendingTransfers(int id)
        {
            IList<Transfer> pendingList = new List<Transfer>();

            string query = "SELECT transfer_id, transfer_status_id, transfer_type_id, account_from, account_to, amount " +
                            "FROM transfer " +
                            "JOIN account ON transfer.account_from = account.user_id AND transfer.account_to = account.user_id " +
                            "JOIN tenmo_user ON account.user_id = tenmo_user.user_id " +
                            "WHERE transfer.transfer_status_id = 1 AND transfer.account_from = @id  OR transfer.account_to = @id;";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Transfer transfer = MapRowToTransfer(reader);
                        pendingList.Add(transfer);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }
            return pendingList;
        }

        /// <summary>
        /// Updates the transfer status of a specified transfer to the chosen status code. 
        /// Status Code 1: Pending
        /// Status Code 2: Approved
        /// Status Code 3: Rejected
        /// </summary>
        /// <param name="transferId">Transfer id of the Transfer to be changed.</param>
        /// <param name="statusCode">New status code</param>
        /// <returns></returns>
        /// <exception cref="DaoException"></exception>
        public Transfer SetTransferStatus(int transferId, int statusCode)
        {
            Transfer updatedTransfer = null;

            string query = "UPDATE transfer " +
                            "SET transfer_status_id = @statusCode " +
                            "WHERE transfer_id = @id;";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@statusCode", statusCode);
                    cmd.Parameters.AddWithValue("@id", transferId);

                    int numberOfRows = cmd.ExecuteNonQuery();
                    if (numberOfRows == 0)
                    {
                        throw new DaoException("Zero rows affected, expected at least one");
                    }
                }
                updatedTransfer = GetTransferById(transferId);
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return updatedTransfer;
        }

        /// <summary>
        /// Maps a Transfer table row to a Transfer object.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Transfer MapRowToTransfer(SqlDataReader reader)
        {
            Transfer trans = new Transfer();
            trans.TransferId = Convert.ToInt32(reader["transfer_id"]);
            trans.TransferType = Convert.ToInt32(reader["transfer_type_id"]);
            trans.TransferStatus = Convert.ToInt32(reader["transfer_status_id"]);
            trans.AccountFromId = Convert.ToInt32(reader["account_from"]);
            trans.AccountToId = Convert.ToInt32(reader["account_to"]);
            trans.TransactionAmount = Convert.ToDecimal(reader["amount"]);

            return trans;
        }

    }
}
