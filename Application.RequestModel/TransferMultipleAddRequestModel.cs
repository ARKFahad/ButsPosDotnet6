using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class TransferMultipleAddRequestModel
    {
        public TransferMultipleAddRequestModel()
        {
            TransferDetails=new List<TransferMultipleDetailModel>();
        }
        public string? TransferNo { get; set; }
        public string? SourceId { get; set; }
        public string? DestinationId { get; set; }
        public string? SupplierId { get; set; }
        public decimal TotalAmount { get; set; }

        public IList<TransferMultipleDetailModel> TransferDetails { get; set; }
    }
}
