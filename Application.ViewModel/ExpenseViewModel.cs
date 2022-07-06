using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class ExpenseViewModel
    {
        public ExpenseViewModel(Expense expense)
        {
            Id = expense.Id;
            ExpenseNo = expense.ExpenseNo;
            EntryDate = expense.EntryDate;
            ExpenseTypeId = expense.ExpenseTypeId;
            BranchId = expense.BranchId;
            Amount = expense.Amount;
            Remark = expense.Remark;
            CreatedOn = expense.CreatedOn;
            UpdatedOn = expense.UpdatedOn;
            CreatedBy = expense.CreatedBy;
            UpdatedBy = expense.UpdatedBy;
            ExpenseType = expense.ExpenseType != null ? new ExpenseTypeViewModel(expense.ExpenseType) : null;
            Branch = expense.Branch != null ? new BranchViewModel(expense.Branch) : null;
        }

        public string Id { get; set; }
        public string ExpenseNo { get; set; }
        public DateTime EntryDate { get; set; }
        public string ExpenseTypeId { get; set; }
        public string BranchId { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public ExpenseTypeViewModel ExpenseType { get; set; }
        public BranchViewModel Branch { get; set; }
    }
}
