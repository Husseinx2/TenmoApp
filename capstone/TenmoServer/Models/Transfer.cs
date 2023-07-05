namespace TenmoServer.Models
{
    public class Transfer
    {
        public int TransferId { get; set; }
        public int TransferTypeId { get; set; }
        public int TransferStatusId { get; set; }
        public int AccountTo { get; set; }
        public int AccountFrom { get; set; }
        public decimal Amount { get; set; }
    }

    public class TransferStatus
    {
        public int Id { get; set; }
        public string Desc { get; set; }
    }

    public class TransferType
    {
        public int Id { get; set; }
        public string Desc { get; set; }
    }
}
