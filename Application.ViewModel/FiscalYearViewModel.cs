using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
  public  class FiscalYearViewModel
    {
        public FiscalYearViewModel(FiscalYear fiscalYear)
        {
            Id = fiscalYear.Id;
            Name = fiscalYear.Name;
            StartDate = fiscalYear.StartDate;
            EndDate = fiscalYear.EndDate;
            IsActive = fiscalYear.IsActive;
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

    }
}
