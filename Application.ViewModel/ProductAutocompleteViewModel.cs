using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class ProductAutocompleteViewModel
    {
        public string Id { get; set; }
        public string Barcode { get; set; }
        public string ProductName { get; set; }
        public decimal PurchaseVat { get; set; }
        public bool IsVatIncludedInPurchasePrice { get; set; } 
        public decimal PurchasePrice { get; set; }
        public decimal Mrp { get; set; }
    }
}
