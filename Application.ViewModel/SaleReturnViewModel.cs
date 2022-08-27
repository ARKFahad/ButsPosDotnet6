using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class SaleReturnViewModel
    {
        public SaleReturnViewModel(SaleReturn saleReturn)
        {
            Id = saleReturn.Id;
            SaleNo = saleReturn.ReferenceNo;
            CustomerId = saleReturn.CustomerId;
            CustomerName = saleReturn.Customer?.Name;
            SaleReturnDate = saleReturn.SaleReturnDate;
            BranchId = saleReturn.BranchId;
            BranchName = saleReturn.Branch?.Name;
            Subtotal = saleReturn.TotalValue;
            TotalItem = saleReturn.TotalItem;
            Discount = saleReturn.Discount;
            CreatedOn = saleReturn.CreatedOn;
            UpdatedOn = saleReturn.UpdatedOn;
            CreatedBy = saleReturn.CreatedBy;
            UpdatedBy = saleReturn.UpdatedBy;
            SaleReturnItems = new List<SaleReturnItemViewModel>();
            if (saleReturn.SaleReturnItems.Any())
            {
                foreach (var item in saleReturn.SaleReturnItems)
                {
                    SaleReturnItems.Add(new SaleReturnItemViewModel(item));
                }
            }
        }

        public string Id { get; set; }
        public string SaleNo { get; set; }
        public string CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal Discount { get; set; }
        public DateTime SaleReturnDate { get; set; }
        public string BranchId { get; set; }
        public string? BranchName { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal Subtotal { get; set; }
        public int TotalItem { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        public IList<SaleReturnItemViewModel> SaleReturnItems { get; set; }
    }
}

