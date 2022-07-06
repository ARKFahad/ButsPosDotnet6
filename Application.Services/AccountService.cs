using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.ViewModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly GHP_POS_DBContext _context;
        public AccountService(GHP_POS_DBContext context)
        {
            _context = context;
        }
        private IList<string> GetAllLevelSubAccounts(string accountId, string branchId)
        {
            var param = new SqlParameter[] {
                      new SqlParameter() {
                            ParameterName = "@parentId",
                            SqlDbType =  System.Data.SqlDbType.NVarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = accountId?? Convert.DBNull
                        }};
            var accountHeads = _context.AccountHeadReportLines.FromSqlRaw("[dbo].[spAllSubAccountsByParentId] @parentId", param).AsEnumerable();
            //asset or current asset or debtor
            //if ((accountId == "1" || accountId == "2" || accountId == "7b6ab461-4a10-4bbc-94a6-13d73ddfd4d4") && !string.IsNullOrWhiteSpace(branchId))
            //{
            //    var idsWithBranch = accountHeads.Where(x => x.BranchId == branchId).Select(x => x.Id).ToList();
            //    var idsWithoutBranch = accountHeads.Where(x => string.IsNullOrWhiteSpace(x.BranchId)).Select(x => x.Id).ToList();
            //    return idsWithoutBranch.Union(idsWithBranch).ToList();
            //}

            return accountHeads.Select(x => x.Id).ToList();
        }
        private decimal CalculateBalanceByAccountId(string accountId, string type, string fiscalYearId, DateTime? fromDate, DateTime? toDate, string branchId = null)
        {
            var accountIds = GetAllLevelSubAccounts(accountId, branchId);
            var transactions = _context.AccountEntries.Where(x => x.FiscalYearId == fiscalYearId).Where(x => accountIds.Contains(x.AccountId));
            if (!string.IsNullOrWhiteSpace(branchId))
                transactions = transactions.Where(x => x.BranchId == branchId);
            if (fromDate.HasValue && toDate.HasValue)
                transactions = transactions.Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);

            if (type == AccType.Liability || type == AccType.Income) return transactions.Sum(x => x.Credit) - transactions.Sum(x => x.Debit);
            return transactions.Sum(x => x.Debit) - transactions.Sum(x => x.Credit);
        }
     
        public decimal GetAccountBalance(string accountId, string type, string branchId = null)
        {
           return  CalculateBalanceByAccountId(accountId, type, _context.FiscalYears.Where(x => x.IsActive)?.FirstOrDefault()?.Id, null, null, branchId);
           
        }
    }
}
