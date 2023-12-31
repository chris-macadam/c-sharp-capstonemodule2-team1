﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TenmoServer.Exceptions;
using TenmoServer.Models;
using TenmoServer.Security;
using TenmoServer.Security.Models;

namespace TenmoServer.DAO
{
    public class UserSqlDao : IUserDao
    {
        private readonly string connectionString;
        const decimal StartingBalance = 1000M;

        public UserSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public User GetUserById(int userId)
        {
            User user = null;

            string sql = "SELECT user_id, username, password_hash, salt FROM tenmo_user WHERE user_id = @user_id";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        user = MapRowToUser(reader);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return user;
        }

        public User GetUserByUsername(string username)
        {
            User user = null;

            string sql = "SELECT user_id, username, password_hash, salt FROM tenmo_user WHERE username = @username";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        user = MapRowToUser(reader);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return user;
        }

        public List<User> GetUsers()
        {
            List<User> users = new List<User>();

            string sql = "SELECT user_id, username, password_hash, salt " +
                        "FROM tenmo_user;";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        User user = MapRowToUser(reader);
                        users.Add(user);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return users;
        }

        public User CreateUser(string username, string password)
        {
            User newUser = null;

            IPasswordHasher passwordHasher = new PasswordHasher();
            PasswordHash hash = passwordHasher.ComputeHash(password);

            string sql = "INSERT INTO tenmo_user (username, password_hash, salt) " +
                         "OUTPUT INSERTED.user_id " +
                         "VALUES (@username, @password_hash, @salt)";

            int newUserId = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // create user
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password_hash", hash.Password);
                    cmd.Parameters.AddWithValue("@salt", hash.Salt);

                    newUserId = Convert.ToInt32(cmd.ExecuteScalar());

                    // create account
                    cmd = new SqlCommand("INSERT INTO account (user_id, balance) VALUES (@userid, @startBalance)", conn);
                    cmd.Parameters.AddWithValue("@userid", newUserId);
                    cmd.Parameters.AddWithValue("@startBalance", StartingBalance);
                    cmd.ExecuteNonQuery();
                }
                newUser = GetUserById(newUserId);
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return newUser;
        }

        /*
        public decimal GetUserBalance(int userId)
        {
            decimal balance = 0;
            string query = "SELECT SUM(balance) AS totalBalance FROM account WHERE user_id = @id;";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", userId);
                    balance = (decimal)cmd.ExecuteScalar();
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }
            return balance;
        }
        */

        public bool CheckAccountBalance(decimal minAmount, int accountId)
        {
            Account account = GetAccountById(accountId);
            decimal userBalance = account.Balance;
            return userBalance >= minAmount;
        }

        private User MapRowToUser(SqlDataReader reader)
        {
            User user = new User();
            user.UserId = Convert.ToInt32(reader["user_id"]);
            user.Username = Convert.ToString(reader["username"]);
            user.PasswordHash = Convert.ToString(reader["password_hash"]);
            user.Salt = Convert.ToString(reader["salt"]);
            return user;
        }

        public Account GetAccountById(int accountId)
        {
            Account account = null;

            string sql = "SELECT account_id, user_id, balance FROM account WHERE account_id = @account_id";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@account_id", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        account = MapRowToAccount(reader);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return account;
        }

        public Account GetAccountByUserId(int userId)
        {
            Account account = null;

            string sql = "SELECT account_id, user_id, balance FROM account WHERE user_id = @user_id";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        account = MapRowToAccount(reader);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return account;
        }

        private Account MapRowToAccount(SqlDataReader reader)
        {
            Account account = new Account()
            {
                AccountId = Convert.ToInt32(reader["account_id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                Balance = Convert.ToDecimal(reader["balance"])
            };
            
            return account;
        }

        public string GetUsernameByAccountId(int accountId)
        {
            string? username = null;
            string query = "SELECT username " +
                            "FROM tenmo_user " +
                            "JOIN account ON account.user_id = tenmo_user.user_id " +
                            "WHERE account.account_id = @id";

            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", accountId);
                    username = Convert.ToString(cmd.ExecuteScalar());
                }
                catch (SqlException ex) { throw new DaoException("SQL exception occurred", ex); }
            }


            return username;
        }
    }
}
