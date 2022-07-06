using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class GenericNameViewModel
    {
        public GenericNameViewModel(GenericName genericName)
        {
            Id = genericName.Id;
            Name = genericName.Name;
            CategoryId = genericName.CategoryId;
            Description = genericName.Description;
            CreatedOn = genericName.CreatedOn;
            UpdatedOn = genericName.UpdatedOn;
            CreatedBy = genericName.CreatedBy;
            UpdatedBy = genericName.UpdatedBy;
            Category = genericName.Category != null ? new CategoryViewModel(genericName.Category) : null;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string CategoryId { get; set; }

        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public CategoryViewModel Category { get; set; }
    }
}
