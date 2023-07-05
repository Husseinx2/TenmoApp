using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IAccountDao
    {
       public decimal GetBalanceByUserID(int userId);
        public decimal GetBalanceByAccountID(int accountId);
        public decimal IncrementBalance(int accountId, decimal amount);
        public int GetAccountId(int userId);
    }
}