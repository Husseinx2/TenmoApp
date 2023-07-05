using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TenmoServer.Exceptions;
using TenmoServer.Models;
using TenmoServer.Security;
using TenmoServer.Security.Models;

namespace TenmoServer.DAO
{
    public class TransferSqlDao : ITransferDao
    {
        private readonly string connectionString;

        public TransferSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

         public Transfer CreateTransfer(Transfer transfer)
        { 
            string sql = "INSERT INTO transfer (transfer_type_id, transfer_status_id, account_from , account_to, amount) " +
                "OUTPUT INSERTED.transfer_id VALUES (@transfer_type_id, @transfer_status_id, @account_from, @account_to, @amount)";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@transfer_type_id", transfer.TransferTypeId);
                        cmd.Parameters.AddWithValue("@transfer_status_id", transfer.TransferStatusId);
                        cmd.Parameters.AddWithValue("@account_from", transfer.AccountFrom);
                        cmd.Parameters.AddWithValue("@account_to", transfer.AccountTo);
                        cmd.Parameters.AddWithValue("@amount", transfer.Amount);

                        transfer.TransferId = (int)cmd.ExecuteScalar();

                    }
                }
            }
            catch(SqlException)
            {
                throw new DaoException();
            }

            return transfer;
        }


        private Transfer MapRowToTransfer(SqlDataReader reader)
        {
            Transfer transfer = new Transfer();
            transfer.TransferId = Convert.ToInt32(reader["transfer_id"]);
            transfer.TransferTypeId = Convert.ToInt32(reader["transfer_type_id"]);
            transfer.TransferStatusId = Convert.ToInt32(reader["transfer_status_id"]);
            transfer.AccountFrom = Convert.ToInt32(reader["account_from"]);
            transfer.AccountTo = Convert.ToInt32(reader["account_to"]);
            transfer.Amount = Convert.ToDecimal(reader["amount"]);

            return transfer;
        }
    }
}
