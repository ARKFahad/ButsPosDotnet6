using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class PurchaseReturnModel
    {
        public PurchaseReturnModel()
        {
            PurchaseItems = new List<PurchaseReturnItemModel>();
        }
        public string? Id { get; set; }
        public string? PurchaseNo { get; set; }
        public string? BranchId { get; set; }

        public string? SupplierId { get; set; }

        public string? Reason { get; set; }

        public decimal NetAmount { get; set; }
       // public decimal Cogs { get; set; }
        public decimal Subtotal { get; set; }

        public string? createdBy { get; set; }



        public IList<PurchaseReturnItemModel> PurchaseItems { get; set; }


    }
}
