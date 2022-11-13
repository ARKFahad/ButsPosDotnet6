using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class TransferDetailViewModel
    {
        public TransferDetailViewModel(TransferDetail transferDetail)
        {
            Id = transferDetail.Id;
            TransferMultipleId=transferDetail.TransferMultipleId;
            SourceBranchName = transferDetail?.TransferMultiple?.Source?.Name;
            DestinationBranchName = transferDetail?.TransferMultiple?.Destination?.Name;
            ProductName =transferDetail?.Product?.Name+" "+transferDetail?.Product?.PackSize;
            TransferQty = transferDetail.TransferQty;
            Amount = transferDetail.Amount;
            CreatedOn = transferDetail.CreatedOn;
        }
        public string Id { get; set; }
        public string TransferMultipleId { get; set; }
        public string? SourceBranchName { get; set; }
        public string? DestinationBranchName { get; set; }
        public string? ProductName { get; set; }
        public int TransferQty { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
