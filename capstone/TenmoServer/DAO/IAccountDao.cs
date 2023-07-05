using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IAccountDao
    {
       public decimal GetBalance(int userId);
       public int GetAccountId(int userId);
    }
}