using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class SupplierPaymentViewModel
    {
        public SupplierPaymentViewModel(SupplierPayment supplierPayment)
        {
            Id = supplierPayment.Id;
            BillNo = supplierPayment.BillNo;
            PaymentDate = supplierPayment.PaymentDate;
            SupplierId = supplierPayment.SupplierId;
            BranchId = supplierPayment.BranchId;
            BranchName = supplierPayment.Branch?.Name;
            Remark = supplierPayment.Remark;
            Supplier = supplierPayment.Supplier != null ? new SupplierViewModel(supplierPayment.Supplier) : null;
            PaymentMethodId = supplierPayment.PaymentMethodId;
            PaymentMethod = supplierPayment.PaymentMethod != null ? new PaymentMethodViewModel(supplierPayment.PaymentMethod) : null;
            PaymentAmount = supplierPayment.PaymentAmount;
            SupplierPreviousBalance = supplierPayment.SupplierPreviousBalance;
            SupplierBalance = supplierPayment.SupplierBalance;
            CreatedOn = supplierPayment.CreatedOn;
            UpdatedOn = supplierPayment.UpdatedOn;
            CreatedBy = supplierPayment.CreatedBy;
            UpdatedBy = supplierPayment.UpdatedBy;

        }

        public string Id { get; set; }
        public string BillNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public string SupplierId { get; set; }
        public string BranchId { get; set; }
        public string? BranchName { get; set; }
        public string PaymentMethodId { get; set; }
        public decimal PaymentAmount { get; set; }
        public string? Remark { get; set; }
        public decimal SupplierPreviousBalance { get; set; }
        public decimal SupplierBalance { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public SupplierViewModel? Supplier { get; set; }
        public PaymentMethodViewModel? PaymentMethod { get; set; }
    }
}
