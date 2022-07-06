using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class SubsidiaryLedgerViewModel
    {
        
        public string? Vnumber{ get; set; }
        public DateTime VoucherDate { get; set; }
        public string? Particular { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        

    }
}
