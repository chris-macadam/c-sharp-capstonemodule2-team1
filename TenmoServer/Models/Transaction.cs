using System.Security.Principal;

namespace TenmoServer.Models
{
    public class Transaction
    {
        public int TransferType { get; set; }
        public int TransferStatus { get; set; }
        public int AccountFromId { get; set; }
        public string? AccountFromName { get; set; }
        public int AccountToId { get; set; }
        public string? AccountToName { get; set; }
        public decimal TransactionAmount { get; set; }
    }
}
