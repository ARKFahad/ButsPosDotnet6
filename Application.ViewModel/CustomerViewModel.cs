using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class CustomerViewModel
    {
        public CustomerViewModel(Customer customer)
        {
            Id = customer.Id;
            Code = customer.Code;
            Name = customer.Name;
            Email = customer.Email;
            Contact = customer.Contact;
            Address = customer.Address;
            Balance = customer.Balance;
            AccountId = customer.AccountId;
            BranchId = customer.BranchId;
            CreatedOn = customer.CreatedOn;
            UpdatedOn = customer.UpdatedOn;
            CreatedBy = customer.CreatedBy;
            UpdatedBy = customer.UpdatedBy;
        }
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public decimal Balance { get; set; }
        public string AccountId { get; set; }
        public string BranchId { get; set; }
        public bool IsAccountActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
