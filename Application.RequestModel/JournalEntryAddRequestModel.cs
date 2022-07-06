using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
   public class JournalEntryAddRequestModel
    {
        public JournalEntryAddRequestModel()
        {
            JournalEntryLines = new List<JournalEntryLineRequestModel>();
        }
        public string? Id { get; set; }
        public string? FiscalYearId { get; set; }
        public string? BranchId { get; set; }
        public string? Vnumber { get; set; }
        public string? Description { get; set; }
        public string? TransactionDate { get; set; }
        public bool IsApproved { get; set; }
        public string? ApprovedBy { get; set; }
       
        public IList<JournalEntryLineRequestModel> JournalEntryLines { get; set; }

    }
}
