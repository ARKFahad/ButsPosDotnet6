using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class JournalEntryViewModel
    {
     
        public JournalEntryViewModel(JournalEntry journalEntry)
        {
            Id = journalEntry.Id;
            FiscalYearId = journalEntry.FiscalYearId;
            FiscalYearName = journalEntry.FiscalYear?.Name;
            BranchId = journalEntry.BranchId;
            BranchName = journalEntry.Branch?.Name;
            Vnumber = journalEntry.Vnumber;
            Description = journalEntry.Description;
            TransactionDate = journalEntry.TransactionDate;
            IsApproved = journalEntry.IsApproved;
            ApprovedBy = journalEntry.ApprovedBy;
            CreatedOn = journalEntry.CreatedOn;
            CreatedBy = journalEntry.CreatedBy;
            JournalEntryLines = new List<JournalEntryLineViewModel>();
            if (journalEntry.JournalEntryLines.Any())
            {
                foreach (var item in journalEntry.JournalEntryLines)
                {
                    JournalEntryLines.Add(new JournalEntryLineViewModel(item));
                }
            }
        }

        public string Id { get; set; }
        public string FiscalYearId { get; set; }
        public string FiscalYearName { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string Vnumber { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }

        public IList<JournalEntryLineViewModel> JournalEntryLines { get; set; }
    }
}
