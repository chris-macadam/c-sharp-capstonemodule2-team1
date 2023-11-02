using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace TenmoServer.Models
{
    public class Transfer
    {
        public int TransferId { get; set; }

        [Required(ErrorMessage = "Type of transaction is required.")]
        public int TransferType { get; set; }

        [Required(ErrorMessage = "Transaction Status is required.")]
        public int TransferStatus { get; set; }

        [Required(ErrorMessage = "Account From ID is required.")]
        public int AccountFromId { get; set; }

        public string? AccountFromName { get; set; }

        [Required(ErrorMessage = "Account To ID is required.")]
        public int AccountToId { get; set; }

        public string? AccountToName { get; set; }

        [Required(ErrorMessage = "Amount to be transferred is required.")]
        public decimal TransactionAmount { get; set; }

        public Transfer()
        {

        }
    }
}
