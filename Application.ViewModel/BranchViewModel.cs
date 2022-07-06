using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
  public  class BranchViewModel
    {
        public BranchViewModel(Branch branch)
        {
            Id = branch.Id;
            Name = branch.Name;
            Code = branch.Code; 
            Address = branch.Address;
            MobileNo = branch.MobileNo;
            VatRegNo = branch.VatRegNo;
            CreatedOn = branch.CreatedOn;
            UpdatedOn = branch.UpdatedOn;
            CreatedBy = branch.CreatedBy;
            UpdatedBy = branch.UpdatedBy;

        }
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string MobileNo { get; set; }
        public string VatRegNo { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
