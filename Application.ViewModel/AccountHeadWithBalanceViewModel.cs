using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class AccountHeadWithBalanceViewModel
    {
        
        public string? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int Level { get; set; }
        public string? ParentId { get; set; }
        public decimal Balance { get; set; }
        public decimal Balance1 { get; set; }
        public decimal Balance2 { get; set; }
        public decimal Balance3 { get; set; }
        public decimal Balance4 { get; set; }
        public decimal Balance5 { get; set; }

    }
}
