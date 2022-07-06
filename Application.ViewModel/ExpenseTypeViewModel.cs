using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class ExpenseTypeViewModel
    {
        public ExpenseTypeViewModel(ExpenseType expenseType)
        {
            Id = expenseType.Id;
            Name = expenseType.Name;
            AccountId = expenseType.AccountId;
            CreatedOn = expenseType.CreatedOn;
            UpdatedOn = expenseType.UpdatedOn;
            CreatedBy = expenseType.CreatedBy;
            UpdatedBy = expenseType.UpdatedBy;

        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string AccountId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
