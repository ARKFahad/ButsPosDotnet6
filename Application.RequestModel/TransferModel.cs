using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class TransferModel
    {
        public string? SourceId { get; set; }
        public string? DestinationId { get; set; }
        public string? ProductId { get; set; }
        public int TransferQty { get; set; }
    }
}
