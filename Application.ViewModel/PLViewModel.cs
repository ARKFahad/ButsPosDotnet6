using Application.Core.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class PLViewModel
    {
        public PLViewModel(PLModel pLModel)
        {
            BranchId = pLModel.BranchId;
            Date = pLModel.Date;
            Revenue = pLModel.Revenue;
            COGS = pLModel.COGS;
            Expens = pLModel.Expens;
            PL = pLModel.PL;
        }
        public string? BranchId { get; set; }
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal COGS { get; set; }
        public decimal Expens { get; set; }
        public decimal PL { get; set; }
    }
}
