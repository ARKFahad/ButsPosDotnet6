using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class TransferViewModel
    {
        public TransferViewModel(TransferHistory transferHistory)
        {
            TransferNo = transferHistory.TransferNo;
            Product = transferHistory.Product != null ? new ProductViewModel(transferHistory.Product) : null;
            SourceBranch = transferHistory.Source.Name;
            DestinationBranch = transferHistory.Destination.Name;
            quantity = transferHistory.Quantity;
            Amount = transferHistory.Amount;

        }
        public string TransferNo { get; set; }
        public ProductViewModel? Product { get; set; }

        public string SourceBranch { get; set; }

        public string DestinationBranch { get; set; }

        public int quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
