using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
   public class RegisterRequestModel
    {
        public string? Username { get; set; }
        public string? RoleId { get; set; }
        public string? BranchId { get; set; }
        public string? Password { get; set; }
    }
}
