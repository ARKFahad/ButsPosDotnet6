using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class TransferMultipleDetailModel
    {
        public string? ProductId { get; set; }
        public int TransferQty { get; set; }
        public decimal Amount { get; set; }
    }
}
