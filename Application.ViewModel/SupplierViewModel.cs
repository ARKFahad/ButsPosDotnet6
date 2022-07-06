using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class SupplierViewModel
    {
       
        public SupplierViewModel(Supplier supplier)
        {
            Id = supplier.Id;
            Code = supplier.Code;
            Name = supplier.Name;
            Email = supplier.Email;
            Contact = supplier.Contact;
            Address = supplier.Address;
            Balance = supplier.Balance;
            AccountId = supplier.AccountId;
            OpeningBalance = supplier.OpeningBalance;
            CreatedOn = supplier.CreatedOn;
            UpdatedOn = supplier.UpdatedOn;
            CreatedBy = supplier.CreatedBy;
            UpdatedBy = supplier.UpdatedBy;
            
        }
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public decimal Balance { get; set; }
        public string AccountId { get; set; }
        public bool IsAccountActive { get; set; }
        public decimal OpeningBalance { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        
    }
}
