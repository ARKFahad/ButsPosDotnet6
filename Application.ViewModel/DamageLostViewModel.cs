using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class DamageLostViewModel
    {
        public DamageLostViewModel(DamageLost damageLost)
        {
            Id = damageLost.Id;
            Code = damageLost.Code;
            Product = damageLost.Product != null ? new ProductViewModel(damageLost.Product) : null;
            BranchName = damageLost.Branch?.Name;
            DamageLostQuantity = damageLost.DamageLostQuantity;
            DamageLostAmount = damageLost.DamageLostAmount;
            Reason = damageLost.Reason;
            CreatedOn = damageLost.CreatedOn;

        }
        public string Id { get; set; }
        public string Code { get; set; }
        public ProductViewModel Product { get; set; }
        public string BranchName { get; set; }
        public int DamageLostQuantity { get; set; }
        public decimal DamageLostAmount { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
