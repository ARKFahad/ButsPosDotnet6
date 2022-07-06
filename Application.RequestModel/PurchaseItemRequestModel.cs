using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
   public class PurchaseItemRequestModel
    {
        public PurchaseItemRequestModel()
        {
            GiftItems = new List<GiftItemRequestModel>();
        }
        public string? Id { get; set; }
        public string? PurchaseId { get; set; }
        public string? ProductId { get; set; }
        public decimal PurchasePrice { get; set; }
        public string? ExpireDate { get; set; }
        public string? BatchNo { get; set; }
        public int PurchaseQuantity { get; set; }
        public int BonusQuantity { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalVat { get; set; }
        public decimal SalePrice { get; set; }
        public IList<GiftItemRequestModel> GiftItems { get; set; }


    }
}
