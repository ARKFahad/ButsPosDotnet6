using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
   public class PurchaseAddRequestModel
    {
        public PurchaseAddRequestModel()
        {
            PurchaseItems = new List<PurchaseItemRequestModel>();
        }
        public string? Id { get; set; }
        public string? PurchaseNo { get; set; }
        public string? SupplierId { get; set; }
        public string? ReferenceNo { get; set; }
        public string? PurchaseDate { get; set; }
        public string? BranchId { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalVat { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal AdditionalCost { get; set; }
        public decimal NetAmount { get; set; }
        public int PurchaseType { get; set; }
        public string? Remark { get; set; }

        public IList<PurchaseItemRequestModel> PurchaseItems { get; set; }

    }
}
