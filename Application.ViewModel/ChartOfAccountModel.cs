using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class ChartOfAccountModel
    {
        public ChartOfAccountModel()
        {
            SubAccountHeads = new List<ChartOfAccountModel>();
        }
        public string? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public int Level { get; set; }
        public string? ParentId { get; set; }
        public List<ChartOfAccountModel> SubAccountHeads { get; set; }
    }
}
