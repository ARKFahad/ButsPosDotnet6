using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class MeasurementUnitViewModel
    {

        public MeasurementUnitViewModel(MeasurementUnit measurementUnit)
        {
            Id = measurementUnit.Id;
            Name = measurementUnit.Name;
            CreatedOn = measurementUnit.CreatedOn;
            UpdatedOn = measurementUnit.UpdatedOn;
            CreatedBy = measurementUnit.CreatedBy;
            UpdatedBy = measurementUnit.UpdatedBy;
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
