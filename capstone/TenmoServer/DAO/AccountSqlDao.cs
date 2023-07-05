using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TenmoServer.Exceptions;
using TenmoServer.Models;
using TenmoServer.Security;
using TenmoServer.Security.Models;

namespace TenmoServer.DAO
{
    public class AccountSqlDao : IAccountDao
    {
        private readonly string connectionString;

        public AccountSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public decimal GetBalanceByAccountID(int accountId)
        {
            decimal balance = 0;
            string sql = "SELECT balance FROM account WHERE account_id = @account_id";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@account_id", accountId);
                    balance = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            return balance;
        }

        public decimal GetBalanceByUserID(int userId)
        {
            decimal balance = 0;
            string sql = "SELECT balance FROM account WHERE user_id = @user_id ";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    balance = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            return balance;
        }

        public int GetAccountId(int userId)
        {
            int accountId = 0;
            string sql = "SELECT account_id FROM account WHERE user_id = @user_id ";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    accountId = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return accountId;
        }
        private Account MapRowToAccount(SqlDataReader reader)
        {
            Account account = new Account();
            account.AccountId = Convert.ToInt32(reader["account_id"]);
            account.UserId = Convert.ToInt32(reader["user_id"]);
            account.Balance = Convert.ToDecimal(reader["user_id"]);

            return account;
        }
    }
}
