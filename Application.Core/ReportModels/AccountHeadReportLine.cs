using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.ReportModels
{
   public class AccountHeadReportLine
    {
        public string? ParentId { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? BranchId { get; set; }
    }
}
