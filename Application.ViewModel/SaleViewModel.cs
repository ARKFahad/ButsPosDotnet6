using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class SaleViewModel
    {
        public SaleViewModel(Sale sale)
        {
            Id = sale.Id;
            SaleNo = sale.SaleNo;
            CustomerId = sale.CustomerId;
            CustomerName = sale.Customer?.Name;
            PaymentMethodId = sale.PaymentMethodId;
            SaleDate = sale.CreatedOn;
            BranchId = sale.BranchId;
            BranchName = sale.Branch?.Name;
            BranchCode = sale.Branch?.Code;          
            Subtotal = sale.Subtotal;
            Discount = sale.Discount;
            DiscountInPercentage = sale.DiscountInPercentage;
            PayableAmount = sale.PayableAmount;
            TotalSaleReturnedAmount = sale.TotalSaleReturnedAmount;
            PaidAmount = sale.PaidAmount;
            DueAmount = sale.DueAmount;
            CreatedOn = sale.CreatedOn;
            UpdatedOn = sale.UpdatedOn;
            CreatedBy = sale.CreatedBy;
            UpdatedBy = sale.UpdatedBy;
            SaleItems = new List<SaleItemViewModel>();
            if (sale.SaleItems.Any())
            {
                foreach (var item in sale.SaleItems)
                {
                    SaleItems.Add(new SaleItemViewModel(item));
                }
            }
        }

        public string Id { get; set; }
        public string SaleNo { get; set; }
        public string CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? PaymentMethodId { get; set; }
        public DateTime SaleDate { get; set; }
        public string BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? BranchCode { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal TotalSaleReturnedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }       
        public decimal Subtotal { get; set; }  
        public decimal Discount { get; set; }
        public decimal DiscountInPercentage { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public IList<SaleItemViewModel> SaleItems { get; set; }
    }
}

