using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class ItemProfitLossRequestModel
    {
        public ItemProfitLossRequestModel()
        {
            Page = 0;
            RowsPerPage = 5;
        }
        public int Page { get; set; }

        public int RowsPerPage { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? BranchId { get; set; }
        public string? SupplierId { get; set; }
        public string? ProductId { get; set; }
       
    }
}
