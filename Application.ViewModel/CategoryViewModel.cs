using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class CategoryViewModel
    {
        public CategoryViewModel(Category category)
        {
            Id = category.Id;
            Name = category.Name;
            CreatedOn = category.CreatedOn;
            UpdatedOn = category.UpdatedOn;
            CreatedBy = category.CreatedBy;
            UpdatedBy = category.UpdatedBy;

        }
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
