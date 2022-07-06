using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class UserViewModel
    {
        public UserViewModel(User user)
        {
            Id = user.Id;
            Username = user.Username;
            RoleId = user.RoleId;
            RoleName = user.Role?.Name;
            BranchId = user.BranchId;
            BranchName = user.Branch?.Name;
            CreatedOn = user.CreatedOn;
            UpdatedOn = user.UpdatedOn;
            CreatedBy = user.CreatedBy;
            UpdatedBy = user.UpdatedBy;
        }
        public string Id { get; set; }
        public string? Username { get; set; }
        public string RoleId { get; set; }
        public string? RoleName { get; set; }
        public string BranchId { get; set; }
        public string? BranchName { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }


    }
}
