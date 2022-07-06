using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class CountryViewModel
    {

        public CountryViewModel(Country country)
        {
            Id = country.Id;
            Name = country.Name;
            CreatedOn = country.CreatedOn;
            UpdatedOn = country.UpdatedOn;
            CreatedBy = country.CreatedBy;
            UpdatedBy = country.UpdatedBy;
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
