using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class SalesData
    {
        public DateTime Date { get; set; }
        public decimal Payable { get; set; }
        public decimal Paid { get; set; }
        public decimal Due { get; set; }
        public decimal ReturnAmount { get; set; }
    }
}
