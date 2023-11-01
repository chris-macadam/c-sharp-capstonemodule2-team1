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

        public Transfer GetTransferById(int id)
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
                    cmd.Parameters.AddWithValue("@transID", id);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        transfer = MapRowToTransaction(reader);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }
            return transfer;
        }

        public IList<Transfer> GetAllTransfer()
        {
            IList<Transfer> transferList = new List<Transfer>();

            string query = "SELECT " +
                            "FROM transfer "
        }

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

                newTransaction = GetTransferByID(newTranferId);
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }
            return newTransaction;
        }

        private Transfer MapRowToTransaction(SqlDataReader reader)
        {
            Transfer trans = new Transfer();
            trans.TransferId = Convert.ToInt32(reader["transfer_id"]);
            trans.TransferType = Convert.ToInt32(reader["transfer_type_id"]);
            trans.TransferStatus = Convert.ToInt32(reader["transfer_status_id"]);
            trans.AccountFromId = Convert.ToInt32(reader["account_from"]);
            trans.AccountToId = Convert.ToInt32(reader["account_to"]);
            trans.TransactionAmount = Convert.ToDecimal(reader["amount"]);
        }

    }
}
