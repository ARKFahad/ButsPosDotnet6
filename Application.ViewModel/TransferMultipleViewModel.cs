using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class TransferMultipleViewModel
    {
        public TransferMultipleViewModel(TransferMultiple transfer)
        {
            Id = transfer.Id;
            TransferNo = transfer.TransferNo;
            SourceId = transfer.SourceId;
            SourceName=transfer.Source.Name;    
            DestinationId = transfer.DestinationId;
            DestinationName = transfer.Destination.Name;
            SupplierId = transfer.SupplierId;
            SupplierName = transfer.Supplier.Name;
            TotalAmount = transfer.TotalAmount;
            CreatedOn = transfer.CreatedOn;
            UpdatedOn = transfer.UpdatedOn;
            CreatedBy = transfer.CreatedBy;
            UpdatedBy = transfer.UpdatedBy;
            TransferDetails = new List<TransferDetailViewModel>();
            if (transfer.TransferDetails.Any())
            {
                foreach (var item in transfer.TransferDetails)
                {
                    TransferDetails.Add(new TransferDetailViewModel(item));
                }
            }
        }
            
        public string Id { get; set; }
        public string TransferNo { get; set; }
        public string SourceId { get; set; }
       public string SourceName { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string DestinationId { get; set; }
        public string DestinationName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public virtual IList<TransferDetailViewModel> TransferDetails { get; set; }
    }
}
