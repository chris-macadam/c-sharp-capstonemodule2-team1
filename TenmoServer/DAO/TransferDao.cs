using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
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

            string query = "SELECT transfer_id, transfer_type_id, transfer_status_id, account_from, account_to, amount, created_by " +
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
        /// <param name="userId">The account identification number being searched for.</param>
        /// <returns>A List of Transfer objects.</returns>
        /// <exception cref="DaoException"></exception>
        public List<Transfer> GetAllTransfers(int userId)
        {
            List<Transfer> transferList = new List<Transfer>();

            string query = "SELECT DISTINCT transfer_id, transfer_status_id, transfer_type_id, account_from, account_to, amount, created_by " +
                            "FROM transfer " +
                            "JOIN account ON transfer.account_from = account.account_id OR transfer.account_to = account.account_id " +
                            "JOIN tenmo_user ON account.user_id = tenmo_user.user_id " +
                            "WHERE account_from = " +
                                "(SELECT account_id FROM account WHERE user_id = @id) " +
                            "OR account_to = " +
                                "(SELECT account_id FROM account WHERE user_id = @id);";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", userId);

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
        /// <param name="transfer">transfer request to be created</param>
        /// <param name="userId">The account identification for the user creating the request.</param>
        /// <returns>Returns a Transfer object with the transaction details.</returns>
        /// <exception cref="DaoException"></exception>
        public Transfer CreateTransfer(Transfer transfer, int userId)
        {
            Transfer newTransaction = null;

            string query = "INSERT INTO transfer (transfer_type_id, transfer_status_id, account_from, account_to, amount, created_by) " +
                            "OUTPUT inserted.transfer_id " +
                            "VALUES (@transferType, @transferStatus, @accountFrom, @accountTo, @amount, @createdBy)";

            int newTranferId = 0;
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@transferType", transfer.TransferType);
                    cmd.Parameters.AddWithValue("@transferStatus", transfer.TransferStatus);
                    cmd.Parameters.AddWithValue("@accountFrom", transfer.AccountFromId);
                    cmd.Parameters.AddWithValue("@accountTo", transfer.AccountToId);
                    cmd.Parameters.AddWithValue("@amount", transfer.TransactionAmount);
                    cmd.Parameters.AddWithValue("@createdBy", userId);

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
        /// <param name="userId">Account identification number for the active user.</param>
        /// <returns>Returns a list of Transfer objects.</returns>
        /// <exception cref="DaoException"></exception>
        public List<Transfer> GetPendingTransfers(int userId)
        {
            List<Transfer> pendingList = new List<Transfer>();

            string query = "SELECT DISTINCT transfer_id, transfer_status_id, transfer_type_id, account_from, account_to, amount, created_by " +
                            "FROM transfer " +
                            "JOIN account ON transfer.account_from = account.account_id OR transfer.account_to = account.account_id " +
                            "JOIN tenmo_user ON account.user_id = tenmo_user.user_id " +
                            "WHERE transfer.transfer_status_id = 1 " +
                                "AND transfer.account_from IN " +
                                    "(SELECT account_id FROM account where user_id = @id) " +
                                "OR transfer.account_to IN " +
                                    "(SELECT account_id FROM account where user_id = @id);";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", userId);

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
        /// <returns>Returns updated status if successful, null if not.</returns>
        /// <exception cref="DaoException"></exception>
        public Transfer SetTransferStatus(int transferId, int statusCode)
        {
            Transfer updatedTransfer = null;

            string query = "UPDATE transfer " +
                            "SET transfer_status_id = @statusCode " +
                            "WHERE transfer_id = @id;";

            string firstQuery = "UPDATE account " +
                                        "SET balance -= @amount " +
                                        "WHERE account_id = @accountFrom;";

            string secQuery = "UPDATE account " +
                                "SET balance += @amount " +
                                "WHERE account_id = @accountTo;";

            // two queries to actually move money
            //open transaction for sql
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    
                    var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@statusCode", statusCode);
                    cmd.Parameters.AddWithValue("@id", transferId);

                    int numberOfRows = cmd.ExecuteNonQuery();
                    if (numberOfRows == 0)
                    {
                        throw new DaoException("Zero rows affected, expected at least one");
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new DaoException("SQL exception occurred", ex);
                }
            }
            updatedTransfer = GetTransferById(transferId);

            if (statusCode == 2)
            {
                TransferFunds(updatedTransfer);
            }
            return updatedTransfer;
        }

        public bool TransferFunds(Transfer transfer)
        {
            string firstQuery = "UPDATE account " +
                                        "SET balance -= @amount " +
                                        "WHERE account_id = @accountFrom;";

            string secQuery = "UPDATE account " +
                                "SET balance += @amountTo " +
                                "WHERE account_id = @accountTo;";

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    cmd.Connection = conn;
                    cmd.Transaction = transaction;

                    cmd.CommandText = firstQuery;
                    cmd.Parameters.AddWithValue("@amount", transfer.TransactionAmount);
                    cmd.Parameters.AddWithValue("@accountFrom", transfer.AccountFromId);
                    int numberOfRows = cmd.ExecuteNonQuery();
                    if (numberOfRows == 0)
                    {
                        throw new DaoException("Zero rows affected, expected at least one");
                    }

                    cmd.CommandText = secQuery;
                    cmd.Parameters.AddWithValue("@amountTo", transfer.TransactionAmount);
                    cmd.Parameters.AddWithValue("accountTo", transfer.AccountToId);
                    numberOfRows = cmd.ExecuteNonQuery();
                    if (numberOfRows == 0)
                    {
                        throw new DaoException("Zero rows affected, expected at least one");
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new DaoException("SQL exception occurred", ex);
                }
            }
            return true;
            }

        public string GetUsernameFromUserId(int userId)
        {
            string query = "SELECT username FROM tenmo_user WHERE user_id = @id;";
            string username;
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", userId);
                    username = (string)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw new DaoException("SQL exeption occurred", ex);
                }
            }
            return username;
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
            trans.CreatedBy = Convert.ToInt32(reader["created_by"]);

            return trans;
        }

    }
}
