using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Common
{
    public class StockModel
    {
        public string? ProductId { get; set; }
        public string? SupplierId { get; set; }       
        public string? Code { get; set; }
        public string? BranchId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductPackSize { get; set; }
        public string? CategoryId { get; set; }
        public string? ProductCategory { get; set; }
        public string? ProductBarcode { get; set; }
        public decimal Mrp { get; set; }
        public string? SupplierName { get; set; }
        public string? BranchName { get; set; }
        public int Quantity { get; set; }
        public decimal Value { get; set; }
    }
}
