using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class RoleViewModel
    {
        public RoleViewModel(Role role)
        {
            Id = role.Id;
            Name = role.Name;
            CreatedOn = role.CreatedOn;
            UpdatedOn = role.UpdatedOn;
            CreatedBy = role.CreatedBy;
            UpdatedBy = role.UpdatedBy;
        }
        public string Id { get; set; }
        public string Name { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
