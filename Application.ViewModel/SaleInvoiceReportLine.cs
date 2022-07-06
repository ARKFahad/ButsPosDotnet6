using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class SaleInvoiceReportLine
    {
        public string BranchName { get; set; }
		public string BranchAddress { get; set; }
		public string BranchMobileNo { get; set; }
		public string BranchVatRegNo { get; set; }
		public string SaleNo { get; set; }
		public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string CustomerId { get; set; }
		public string CustomerName { get; set; }
		public string CustomerAddress { get; set; }
		public string CustomerContact { get; set; }
		public List<SaleItemReportLine> saleItems { get; set; }
		public decimal Subtotal { get; set; }
		public decimal Discount { get; set; }
		public decimal PayableAmount { get; set; }
		
		
	}
}
public class SaleItemReportLine
{
	public string ProductId { get; set; }
	public string ProductName { get; set; }
	public string ProductPackSize { get; set; }
	public string CategoryName { get; set; }
	public int Quantity { get; set; }
	public decimal Price { get; set; }
	public decimal Amount { get; set; }
}