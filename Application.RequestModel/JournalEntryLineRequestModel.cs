using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
   public class JournalEntryLineRequestModel
    {

        public string? Id { get; set; }
        public string? JournalEntryId { get; set; }
        public string? AccountId { get; set; }
        public string? PostType { get; set; }
        public decimal Amount { get; set; }
        public bool IsOpeningBalance { get; set; }


    }
}
