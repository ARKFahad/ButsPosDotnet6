using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.RequestModel;
using Application.Services;
using Application.ViewModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IPdfService _pdfService;
        public AccountController(GHP_POS_DBContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }
        [HttpGet("account-heads")]
        public IActionResult GetAccountHeads()
        {
            return Ok(PrepareAccountHeadWithParents());
        }
        [HttpGet("balance/{accountId}/{type}/{branchId}")]
        public IActionResult GetAccountBalance(string accountId, string type, string? branchId = null)
        {
            var balance = CalculateBalanceByAccountId(accountId, type, _context.FiscalYears.FirstOrDefault(x=>x.IsActive)?.Id, null, null, branchId);
            return Ok(new { Balance = balance });
        }
        [HttpGet("control-accounts")]
        public IActionResult GetControlAccountHeads(string? branchId)
        {
            return Ok(PrepareControlAccountHeadWithParents(branchId));
        }
        [Authorize(Policy = Permissions.AccountingReports.ChartOfAccounts)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("chart-of-accounts")]
        public IActionResult ChartOfAccount(string? rootAccountHeadId = null, int level = 0, int levelsToLoad = 5)
        {
            return Ok(PrepareAccountHeadModels(rootAccountHeadId, level, levelsToLoad));
        }
        [Authorize(Policy = Permissions.AccountingReports.SubsidiaryLedger)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("subsidiary-ledger-print")]
        public virtual IActionResult SubsidiaryReportPrint(string? accountId = null, DateTime? fromDate = null, DateTime? toDate = null, string? branchId = null)
        {
            try
            {
                var list = SubsidiaryLedger(accountId, fromDate, toDate, branchId);
                var account = _context.AccountHeads.Find(accountId);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                   _pdfService.PrintSubsidiaryLedgerReportToPdf(stream, _context.Branches.Find(branchId), list, "Subsidiary Ledger("+account?.Name+")", fromDate, toDate);
                    bytes = stream.ToArray();
                    bytes = _pdfService.AddPageNumbers(bytes);
                }
                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(Policy = Permissions.AccountingReports.TrialBalance_BalanceSheet_IncomeStatement)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("trial-balance-print")]
        public virtual IActionResult TrialBalanceReportPrint(AccountReportRequestModel requestModel)
        {
            try
            {
                requestModel.FiscalYearId = _context.FiscalYears.FirstOrDefault(x => x.IsActive).Id;
                var accountHeadsQueryable = _context.AccountHeads.AsQueryable();
                var list = TrialBalance(accountHeadsQueryable, requestModel.RootAccountHeadId, requestModel.Level, requestModel.LevelsToLoad, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintTrailBalanceReportToPdf(stream, _context.Branches.Find(requestModel.BranchId), list, "Trial Balance ", requestModel.FromDate, requestModel.ToDate);
                    bytes = stream.ToArray();
                    bytes = _pdfService.AddPageNumbers(bytes);
                }
                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }
        [Authorize(Policy = Permissions.AccountingReports.TrialBalance_BalanceSheet_IncomeStatement)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("balance-sheet-print")]
        public IActionResult PrintBalanceSheetReport(AccountReportRequestModel requestModel)
        {
            try
            {
                var fromDate = requestModel.FromDate;
                var fiscalYear = _context.FiscalYears.FirstOrDefault(x => x.IsActive);
                //balance sheet in a particar date from fiscal year start date
                requestModel.FromDate = fiscalYear.StartDate;
                requestModel.FiscalYearId = fiscalYear.Id;
                var income = CalculateBalanceByAccountId(AccountHeadConstants.Income, AccType.Income, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);
                var expense = CalculateBalanceByAccountId(AccountHeadConstants.Expenditure, AccType.Expense, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);
                var accountHeadsQueryable = _context.AccountHeads.Where(x => new string[2] { AccType.Asset, AccType.Liability }.Contains(x.AccountTypeId));
                var list = PrepareAccountHeadModelsWithBalance(accountHeadsQueryable, requestModel.RootAccountHeadId, requestModel.Level, requestModel.LevelsToLoad, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId, income-expense);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintBalanceSheetReportToPdf(stream, _context.Branches.Find(requestModel.BranchId), list, "Balance Sheet", fromDate, requestModel.ToDate,income-expense);
                    bytes = stream.ToArray();
                    bytes = _pdfService.AddPageNumbers(bytes);
                }
                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }

        }
        [Authorize(Policy = Permissions.AccountingReports.TrialBalance_BalanceSheet_IncomeStatement)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("income-statement-print")]
        public IActionResult PrintIncomeStatementReport(AccountReportRequestModel requestModel)
        {
            try
            {
                requestModel.FiscalYearId = _context.FiscalYears.FirstOrDefault(x => x.IsActive).Id;
                var income = CalculateBalanceByAccountId(AccountHeadConstants.Income, AccType.Income, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);
                var expense = CalculateBalanceByAccountId(AccountHeadConstants.Expenditure, AccType.Expense, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);
                var accountHeadsQueryable = _context.AccountHeads.Where(x => new string[2] { AccType.Income, AccType.Expense}.Contains(x.AccountTypeId));
                var list = PrepareAccountHeadModelsWithBalance(accountHeadsQueryable, requestModel.RootAccountHeadId, requestModel.Level, requestModel.LevelsToLoad, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintIncomeStatementReportToPdf(stream, _context.Branches.Find(requestModel.BranchId), list, "Income Statement ", requestModel.FromDate, requestModel.ToDate,income-expense);
                    bytes = stream.ToArray();
                    bytes = _pdfService.AddPageNumbers(bytes);
                }

                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
          
        }


        private IList<ChartOfAccountModel> PrepareAccountHeadModels(string? rootAccountHeadId, int level, int levelsToLoad)
        {
            var result = new List<ChartOfAccountModel>();
            var accountHeadsQueryable = _context.AccountHeads.Include(x=>x.AccountType).AsQueryable();
            foreach (var account in GetAllSubAccountsByParentAccountId(rootAccountHeadId, accountHeadsQueryable))
            {
                var accountModel = new ChartOfAccountModel()
                {
                    Id = account.Id,
                    Code=account.Code,
                    Type=account.AccountType?.Name,
                    Level=account.Level,
                    Name = account.Name,
                    ParentId=account.ParentId
                };
               
                //load sub accounts
                bool loadSubAccounts = true;
                if (levelsToLoad <= level)
                {
                    loadSubAccounts = false;
                }
                if (loadSubAccounts)
                {
                    var subAccounts = PrepareAccountHeadModels(account.Id, level + 1, levelsToLoad);
                    accountModel.SubAccountHeads.AddRange(subAccounts);
                }
                result.Add(accountModel);
            }
            return result;
        }
        private IList<AccountHeadDropdownModel> PrepareAccountHeadWithParents()
        {
            var list = new List<AccountHeadDropdownModel>();
            foreach (var item in _context.AccountHeads.Include(x=>x.AccountType).Where(x=>!x.IsControlAccount).ToList())
            {
                list.Add(new AccountHeadDropdownModel
                {
                    Id=item.Id,
                    Code=item.Code,
                    Name=item.Name,
                    AccountTypeId=item.AccountTypeId,
                    TreeName= GetAccountHeadNameWithParents(item)
                });
            }
            
            return list;
        }
        private IList<AccountHeadDropdownModel> PrepareControlAccountHeadWithParents(string? branchId)
        {
            var list = new List<AccountHeadDropdownModel>();
            var queryable = _context.AccountHeads.Include(x => x.AccountType).Where(x => x.IsControlAccount);
            var controlAccountHeads = queryable.Where(x =>string.IsNullOrWhiteSpace(x.BranchId)).ToList();
            var debtors = queryable.Where(x => x.BranchId == branchId && x.ParentId == AccountHeadConstants.Debtors).ToList();
            
            foreach (var item in string.IsNullOrWhiteSpace(branchId)?queryable.ToList(): controlAccountHeads.Union(debtors))
            {
                list.Add(new AccountHeadDropdownModel
                {
                    Id = item.Id,
                    Code = item.Code,
                    Name=item.Name,
                    TreeName = GetAccountHeadNameWithParents(item)
                });
            }

            return list.OrderBy(x=>x.Name).ToList();
        }
        //asset ==> fixed asset ==> furniture ==> table
        private string GetAccountHeadNameWithParents(AccountHead? accountHead)
        {
            if (accountHead == null)
                throw new ArgumentNullException("accounthead");

            var result = new List<AccountHead>();

            //used to prevent circular references
            var alreadyProcessedAccountHeadIds = new List<string>() { };

            while (accountHead != null && 
                !alreadyProcessedAccountHeadIds.Contains(accountHead.Id)) //prevent circular references
            {
                result.Add(accountHead);
                alreadyProcessedAccountHeadIds.Add(accountHead.Id);
                accountHead = _context.AccountHeads.Find(accountHead.ParentId);
            }
            result.Reverse();
            return string.Join(" > ", result.Select(x => x.Level+"."+x.Name).ToList());
        }

        private IList<AccountHeadWithBalanceViewModel> PrepareAccountHeadModelsWithBalance(IQueryable<AccountHead> queryable, string? rootAccountHeadId, int level, int levelsToLoad,string fiscalYearId,  DateTime? fromDate, DateTime? toDate, string? branchId = null,decimal profitLoss=0)
        {
            var result = new List<AccountHeadWithBalanceViewModel>();
            foreach (var account in GetAllSubAccountsByParentAccountId(rootAccountHeadId, queryable))
            {
                var accountModel = new AccountHeadWithBalanceViewModel()
                {
                    Id = account.Id,
                    Code = account.Code,
                    Name = account.Name,
                    Level=account.Level, 
                    ParentId = account.ParentId
                };
                var accBalance = CalculateBalanceByAccountId(account.Id, account.AccountTypeId,fiscalYearId,fromDate,toDate,branchId);
                if (account.Id == AccountHeadConstants.Equity|| account.Id == AccountHeadConstants.LiabilitiesAndEquity)
                {
                    accBalance = accBalance + profitLoss;
                }
                switch (account.Level)
                {
                    case 1:
                        accountModel.Balance1 = accBalance;
                        break;
                    case 2:
                        accountModel.Balance2 = accBalance;
                        break;
                    case 3:
                        accountModel.Balance3 = accBalance;
                        break;
                    case 4:
                        accountModel.Balance4 = accBalance;
                        break;
                    case 5:
                        accountModel.Balance5 = accBalance;
                        break;
                    default:
                        accountModel.Balance = accBalance;
                        break;
                }
                result.Add(accountModel);
                //load sub accounts
                bool loadSubAccounts = true;
                if (levelsToLoad <= level)
                {
                    loadSubAccounts = false;
                }
                if (loadSubAccounts)
                {
                    var subAccounts = PrepareAccountHeadModelsWithBalance(queryable, account.Id, level + 1, levelsToLoad, fiscalYearId, fromDate, toDate, branchId, profitLoss);
                    result.AddRange(subAccounts);
                }
                
            }
            return result;
        }
        private IList<SubsidiaryLedgerViewModel> SubsidiaryLedger(string? accountId, DateTime? fromDate, DateTime? toDate, string? branchId)
        {
            var subsidiaryLedgerReportLines = new List<SubsidiaryLedgerViewModel>();
            var account = _context.AccountHeads.Find(accountId);
            var fiscalYear = _context.FiscalYears.Where(x => x.IsActive).FirstOrDefault();

            var allTransactions = _context.AccountEntries.Where(x => x.AccountId == accountId)
                .Where(x => x.FiscalYearId == fiscalYear.Id);
            if (!string.IsNullOrWhiteSpace(branchId))
            {
                allTransactions = allTransactions.Where(x => x.BranchId == branchId);
            }
            //opening balance
            var openingBalanceTransactions = allTransactions.Where(x => x.IsOpeningBalance);
            var accountTransactions = allTransactions.Where(x => !x.IsOpeningBalance);
            IQueryable<AccountEntry> transactionsUptoFromDate = new List<AccountEntry>().AsQueryable();
            if (fromDate.HasValue)
                transactionsUptoFromDate = accountTransactions.Where(x => x.TransactionDate.Date >= fiscalYear.StartDate.Date && x.TransactionDate.Date < fromDate.Value.Date);
            //this period transactions
            if (fromDate.HasValue && toDate.HasValue)
                accountTransactions = accountTransactions.Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);
            // var thisPeriodDebit = accountTransactions.Sum(x => x.Debit);
            //var thisPeriodCredit = accountTransactions.Sum(x => x.Credit);
            var openingCredit = openingBalanceTransactions.Sum(x => x.Credit) +
                    (transactionsUptoFromDate.Any() ? transactionsUptoFromDate.Sum(x => x.Credit) : 0M);
            var openingDebit = openingBalanceTransactions.Sum(x => x.Debit) +
                    (transactionsUptoFromDate.Any() ? transactionsUptoFromDate.Sum(x => x.Debit) : 0M);
            accountTransactions = accountTransactions.OrderBy(x => x.TransactionDate);
            
            if (account?.AccountTypeId == AccType.Liability || account?.AccountTypeId == AccType.Income)
            {
                subsidiaryLedgerReportLines.Add(new SubsidiaryLedgerViewModel
                {
                    Vnumber = "",
                    VoucherDate = fromDate.GetValueOrDefault(),
                    Particular = "Opening Balance",
                    Debit = openingDebit,
                    Credit = openingCredit,
                    Balance = openingCredit - openingDebit
                });
                foreach (var item in accountTransactions)
                {
                    openingCredit += item.Credit;
                    openingDebit += item.Debit;
                    subsidiaryLedgerReportLines.Add(new SubsidiaryLedgerViewModel
                    {
                        Vnumber = item.Vnumber,
                        VoucherDate = item.TransactionDate,
                        Particular = item.Description,
                        Debit = item.Debit,
                        Credit = item.Credit,
                        Balance = openingCredit - openingDebit
                    });
                }

            }
            else
            {
                subsidiaryLedgerReportLines.Add(new SubsidiaryLedgerViewModel
                {
                    Vnumber = "",
                    VoucherDate = fromDate.GetValueOrDefault(),
                    Particular = "Opening Balance",
                    Debit = openingDebit,
                    Credit = openingCredit,
                    Balance = openingDebit - openingCredit
                });
                foreach (var item in accountTransactions)
                {
                    openingCredit += item.Credit;
                    openingDebit += item.Debit;
                    subsidiaryLedgerReportLines.Add(new SubsidiaryLedgerViewModel
                    {
                        Vnumber = item.Vnumber,
                        VoucherDate = item.TransactionDate,
                        Particular = item.Description,
                        Debit = item.Debit,
                        Credit = item.Credit,
                        Balance = openingDebit - openingCredit
                    });
                }
            }

            return subsidiaryLedgerReportLines;
        }

        private IList<TrialBalanceViewModel> TrialBalance(IQueryable<AccountHead> queryable, string? rootAccountHeadId, int level, int levelsToLoad, string fiscalYearId, DateTime? fromDate, DateTime? toDate, string? branchId = null)
        {
            var result = new List<TrialBalanceViewModel>();
            foreach (var account in GetAllSubAccountsByParentAccountId(rootAccountHeadId, queryable,branchId))
            {
                var trialBalanceReportLine = PrepareTrialBalanceReportLine(account, fiscalYearId, fromDate, toDate,branchId);
                result.Add(trialBalanceReportLine);
                //load sub accounts
                bool loadSubAccounts = true;
                if (levelsToLoad <= level)
                {
                    loadSubAccounts = false;
                }
                if (loadSubAccounts)
                {
                    var subAccounts = TrialBalance(queryable, account.Id, level + 1, levelsToLoad, fiscalYearId, fromDate, toDate, branchId);
                    result.AddRange(subAccounts);
                }
            }
            return result;
        }
       
        private IList<AccountHead> GetAllSubAccountsByParentAccountId(string? parentAccountId,IQueryable<AccountHead> accountHeads,string? branchId=null)
           
        {
            if (parentAccountId == AccountHeadConstants.Debtors && !string.IsNullOrWhiteSpace(branchId))
              return accountHeads.Where(x => x.BranchId == branchId && x.ParentId==parentAccountId).OrderBy(x => x.Name).ToList();
            if (parentAccountId == null || parentAccountId==AccountHeadConstants.LiabilitiesAndEquity) return accountHeads.Where(x => x.ParentId == parentAccountId).ToList();
              return accountHeads.Where(x => x.ParentId == parentAccountId).OrderBy(x=>x.Name).ToList();
        }
        private IList<string?> GetAllLevelSubAccounts(string accountId)
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

        private decimal CalculateBalanceByAccountId(string accountId,string type,string fiscalYearId, DateTime? fromDate, DateTime? toDate, string? branchId = null)
        {
            var accountIds = GetAllLevelSubAccounts(accountId);
            var transactions = _context.AccountEntries.Where(x => x.FiscalYearId == fiscalYearId).Where(x => accountIds.Contains(x.AccountId));
            if (!string.IsNullOrWhiteSpace(branchId))
                transactions = transactions.Where(x => x.BranchId == branchId);
            if (fromDate.HasValue && toDate.HasValue)
                transactions = transactions.Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);
            if (type==AccType.Liability || type==AccType.Income) return transactions.Sum(x => x.Credit) - transactions.Sum(x => x.Debit);
            return transactions.Sum(x => x.Debit) - transactions.Sum(x => x.Credit);
        }


        private TrialBalanceViewModel PrepareTrialBalanceReportLine(AccountHead account, string fiscalYearId, DateTime? fromDate, DateTime? toDate,string? branchId=null)
        {
            var fiscalYear = _context.FiscalYears.Find(fiscalYearId);
            var accountIds = GetAllLevelSubAccounts(account.Id);
            var allTransactions = _context.AccountEntries.Where(x => accountIds.Contains(x.AccountId))
                .Where(x => x.FiscalYearId == fiscalYearId);
            if (!string.IsNullOrWhiteSpace(branchId))
            {
                allTransactions = allTransactions.Where(x => x.BranchId == branchId);
            }
            //opening balance
            var openingBalanceTransactions = allTransactions.Where(x => x.IsOpeningBalance);
            var accountTransactions = allTransactions.Where(x => !x.IsOpeningBalance);
            IQueryable<AccountEntry> transactionsUptoFromDate= new List<AccountEntry>().AsQueryable() ;
            if (fromDate.HasValue)
             transactionsUptoFromDate = accountTransactions.Where(x => x.TransactionDate.Date >= fiscalYear.StartDate.Date && x.TransactionDate.Date < fromDate.Value.Date);
            //this period transactions
            if (fromDate.HasValue && toDate.HasValue)
                 accountTransactions = accountTransactions.Where(x =>x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);
            var thisPeriodDebit = accountTransactions.Sum(x => x.Debit);
            var thisPeriodCredit = accountTransactions.Sum(x => x.Credit);
            var openingCredit = openingBalanceTransactions.Sum(x => x.Credit) + 
                    (transactionsUptoFromDate.Any() ? transactionsUptoFromDate.Sum(x => x.Credit):0M);
            var openingDebit = openingBalanceTransactions.Sum(x => x.Debit) +
                    (transactionsUptoFromDate.Any() ? transactionsUptoFromDate.Sum(x => x.Debit) : 0M);
            
            if (account.AccountTypeId == AccType.Liability || account.AccountTypeId == AccType.Income) {//2=liability,3=income(credit nature)
                return new TrialBalanceViewModel
                {
                    Id = account.Id,
                    Code = account.Code,
                    Name = account.Name,
                    Level = account.Level,
                    ThisPeriodDebit =thisPeriodDebit,
                    ThisPeriodCredit = thisPeriodCredit,
                    OpeningDebit = 0M,
                    OpeningCredit = openingCredit-openingDebit,
                    BalanceDebit = 0M,
                    BalanceCredit =(thisPeriodCredit-thisPeriodDebit)+ (openingCredit - openingDebit),
                };
            }
            return new TrialBalanceViewModel
            {
                Id = account.Id,
                Code = account.Code,
                Name = account.Name,
                Level = account.Level,
                ThisPeriodDebit = accountTransactions.Sum(x => x.Debit),
                ThisPeriodCredit = accountTransactions.Sum(x => x.Credit),
                OpeningDebit=openingDebit - openingCredit,
                OpeningCredit = 0M,
                BalanceDebit = ( thisPeriodDebit - thisPeriodCredit) + ( openingDebit - openingCredit),
                BalanceCredit=0M
            };
        }

        private void PrepareNewFiscalYear(string newFiscalYearId, string branchId)
        {
            var previousFiscalYear = _context.FiscalYears.First(x => x.IsActive);
            var income = CalculateBalanceByAccountId(AccountHeadConstants.Income, AccType.Income, previousFiscalYear.Id, null, null, branchId);
            var expense = CalculateBalanceByAccountId(AccountHeadConstants.Expenditure, AccType.Expense, previousFiscalYear.Id, null, null, branchId);
            var profitLoss = income - expense;
            var accountHeads = _context.AccountHeads.Where(x =>!x.Deleted && new string[2] { AccType.Asset, AccType.Liability }.Contains(x.AccountTypeId)).ToList();
            foreach (var account in accountHeads)
            {
                var accBalance = CalculateBalanceByAccountId(account.Id, account.AccountTypeId, previousFiscalYear.Id, null, null, branchId);
                if (account.Id == AccountHeadConstants.RetainedEarnings)
                {
                    accBalance = profitLoss;
                }
                _context.AccountEntries.Add(new AccountEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    FiscalYearId = newFiscalYearId,
                    BranchId = branchId,
                    Vnumber = "OP",
                    Description = "Opening Balance from previous Fiscal Year",
                    AccountId = account.Id,
                    Debit = account.AccountTypeId == AccType.Asset?accBalance:0M,
                    Credit = account.AccountTypeId == AccType.Liability ? accBalance : 0M,
                    TransactionDate = DateTime.Now,
                    IsOpeningBalance = true,
                    CreatedBy = "me"
                });
               
            }
            _context.SaveChanges();            
        }
    }
}
