using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface ITransferDao
    {
        Transfer CreateTransfer(Transfer transfer);
        List<Transfer> GetTransfers(int accountId);
        TransferStatus GetTransferStatus(int id);
        TransferType GetTransferType(int id);
        Transfer UpdateTransfer(Transfer transfer);
    }
}