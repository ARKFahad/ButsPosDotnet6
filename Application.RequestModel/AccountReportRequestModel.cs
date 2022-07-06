using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class AccountReportRequestModel
    {
        //  string? rootAccountHeadId = null, int level = 0, int levelsToLoad = 5, string fiscalYearId = "1", DateTime? fromDate = null, DateTime? toDate = null, string? branchId = null
        public string? RootAccountHeadId { get; set; }
        public int Level { get; set; } = 0;
        public int LevelsToLoad { get; set; } = 5;
        public string FiscalYearId { get; set; } = "1";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? BranchId { get; set; }
    }
}
