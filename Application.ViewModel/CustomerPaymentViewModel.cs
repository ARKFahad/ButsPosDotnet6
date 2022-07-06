using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class CustomerPaymentViewModel
    {
        public CustomerPaymentViewModel(CustomerPayment customerPayment)
        {
            Id = customerPayment.Id;
            BillNo = customerPayment.BillNo;
            PaymentDate = customerPayment.PaymentDate;
            CustomerId = customerPayment.CustomerId;
            BranchName = customerPayment.Branch.Name;
            Customer = customerPayment.Customer != null ? new CustomerViewModel(customerPayment.Customer) : null;
            PaymentMethodId = customerPayment.PaymentMethodId;
            PaymentMethod = customerPayment.PaymentMethod != null ? new PaymentMethodViewModel(customerPayment.PaymentMethod) : null;
            PaymentAmount = customerPayment.PaymentAmount;
            CustomerPreviousBalance = customerPayment.CustomerPreviousBalance;
            CustomerBalance = customerPayment.CustomerBalance;
            CreatedOn = customerPayment.CreatedOn;
            UpdatedOn = customerPayment.UpdatedOn;
            CreatedBy = customerPayment.CreatedBy;
            UpdatedBy = customerPayment.UpdatedBy;
            Remark = customerPayment.Remark;
        }

        public string Id { get; set; }
        public string BillNo { get; set; }
        public string BranchName { get; set; }
        public DateTime PaymentDate { get; set; }
        public string CustomerId { get; set; }
        public string PaymentMethodId { get; set; }
        public decimal PaymentAmount { get; set; }
        public string Remark { get; set; }
        public decimal CustomerPreviousBalance { get; set; }
        public decimal CustomerBalance { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public CustomerViewModel? Customer { get; set; }
        public PaymentMethodViewModel? PaymentMethod { get; set; }
    }
}
