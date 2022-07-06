using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class JournalEntryLineViewModel
    {
        public JournalEntryLineViewModel(JournalEntryLine journalEntryLine )
        {
            Id = journalEntryLine.Id;
            JournalEntryId = journalEntryLine.JournalEntryId;
            AccountId = journalEntryLine.AccountId;
            AccountName = journalEntryLine?.Account?.Name;
            Debit = journalEntryLine.Debit;
            Credit = journalEntryLine.Credit;
            IsOpeningBalance = journalEntryLine.IsOpeningBalance;
        }
        public string Id { get; set; }
        public string JournalEntryId { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public bool IsOpeningBalance { get; set; }

    }
}
