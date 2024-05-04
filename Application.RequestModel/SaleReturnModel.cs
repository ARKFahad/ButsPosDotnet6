using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class SaleReturnModel
    {
        public SaleReturnModel()
        {
            SaleItems = new List<SaleReturnitemModel>();
        }
        public string? Id { get; set; }
        public string? SaleNo { get; set; }
        public string? BranchId { get; set; }

        public string? CustomerId { get; set; }

        public string? Reason { get; set; }

        public decimal PayableAmount { get; set; }
        public decimal Cogs { get; set; }
        public decimal Subtotal { get; set; }

        public string? createdBy { get; set; }
        public decimal PaidAmount { get; set; }


        public IList<SaleReturnitemModel> SaleItems { get; set; }


    }
}
