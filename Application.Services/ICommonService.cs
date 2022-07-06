using Application.Core.Data;
using Application.Core.Entities;
using Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface ICommonService
    {
        Task HitAccount(GHP_POS_DBContext context, string branchId, string vnumber, string description, string accountId, decimal debit, decimal credit, DateTime transactionDate, bool isOpeningBalance = false);
        string GetAccountCode(string accountTypeId);
        Task<int> GetAccountLevel(string parentId);
        Task<List<ProductAutocompleteViewModel>> SearchProductAutocomplete(string supplierid);
        decimal GetSalesTotal(int days, string? branchId = "");
        decimal GetSalesTotal(DateTime? createdFrom = null, DateTime? createdTo = null, string? branchId = "");
    }
}
