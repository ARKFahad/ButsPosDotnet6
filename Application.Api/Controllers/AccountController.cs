using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.RequestModel;
using Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IPdfService _pdfService;
        private readonly IAccountService _accountService;

        public AccountController(GHP_POS_DBContext context, IPdfService pdfService, IAccountService accountService)
        {
            _context = context;
            _pdfService = pdfService;
            _accountService = accountService;
        }

        [HttpGet("account-heads")]
        public IActionResult GetAccountHeads()
        {
            return Ok(_accountService.PrepareAccountHeadWithParents());
        }

        [HttpGet("balance/{accountId}/{type}/{branchId}")]
        public IActionResult GetAccountBalance(string accountId, string type, string? branchId = null)
        {
            var balance = _accountService.CalculateBalanceByAccountId(accountId, type, _context.FiscalYears.FirstOrDefault(x => x.IsActive)?.Id, null, null, branchId);
            return Ok(new { Balance = balance });
        }

        [HttpGet("control-accounts")]
        public IActionResult GetControlAccountHeads(string? branchId)
        {
            return Ok(_accountService.PrepareControlAccountHeadWithParents(branchId));
        }

        [Authorize(Policy = Permissions.AccountingReports.ChartOfAccounts)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("chart-of-accounts")]
        public IActionResult ChartOfAccount(string? rootAccountHeadId = null, int level = 0, int levelsToLoad = 5)
        {
            return Ok(_accountService.PrepareAccountHeadModels(rootAccountHeadId, level, levelsToLoad));
        }

        [Authorize(Policy = Permissions.AccountingReports.SubsidiaryLedger)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("subsidiary-ledger-print")]
        public virtual IActionResult SubsidiaryReportPrint(string? accountId = null, DateTime? fromDate = null, DateTime? toDate = null, string? branchId = null)
        {
            try
            {
                var list = _accountService.SubsidiaryLedger(accountId, fromDate, toDate, branchId);
                var account = _context.AccountHeads.Find(accountId);

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintSubsidiaryLedgerReportToPdf(stream, _context.Branches.Find(branchId), list, "Subsidiary Ledger(" + account?.Name + ")", fromDate, toDate);
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
                var list = _accountService.TrialBalance(accountHeadsQueryable, requestModel.RootAccountHeadId, requestModel.Level, requestModel.LevelsToLoad, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);

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
                var income = _accountService.CalculateBalanceByAccountId(AccountHeadConstants.Income, AccType.Income, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);
                var expense = _accountService.CalculateBalanceByAccountId(AccountHeadConstants.Expenditure, AccType.Expense, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);
                var accountHeadsQueryable = _context.AccountHeads.Where(x => new string[2] { AccType.Asset, AccType.Liability }.Contains(x.AccountTypeId));
                var list = _accountService.PrepareAccountHeadModelsWithBalance(accountHeadsQueryable, requestModel.RootAccountHeadId, requestModel.Level, requestModel.LevelsToLoad, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId, income - expense);

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintBalanceSheetReportToPdf(stream, _context.Branches.Find(requestModel.BranchId), list, "Balance Sheet", fromDate, requestModel.ToDate, income - expense);
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
                var income = _accountService.CalculateBalanceByAccountId(AccountHeadConstants.Income, AccType.Income, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);
                var expense = _accountService.CalculateBalanceByAccountId(AccountHeadConstants.Expenditure, AccType.Expense, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);
                var accountHeadsQueryable = _context.AccountHeads.Where(x => new string[2] { AccType.Income, AccType.Expense }.Contains(x.AccountTypeId));
                var list = _accountService.PrepareAccountHeadModelsWithBalance(accountHeadsQueryable, requestModel.RootAccountHeadId, requestModel.Level, requestModel.LevelsToLoad, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintIncomeStatementReportToPdf(stream, _context.Branches.Find(requestModel.BranchId), list, "Income Statement ", requestModel.FromDate, requestModel.ToDate, income - expense);
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
        [HttpPost("income-statement-print-custom")]
        public IActionResult PrintIncomeStatementReportCustom(AccountReportRequestModel requestModel)
        {
            try
            {
                requestModel.FiscalYearId = _context.FiscalYears.FirstOrDefault(x => x.IsActive).Id;
                var income = _accountService.CalculateBalanceByAccountId(AccountHeadConstants.Income, AccType.Income, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);
                var expense = _accountService.CalculateBalanceByAccountId(AccountHeadConstants.Expenditure, AccType.Expense, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);
                var incomeHeadsQueryable = _context.AccountHeads.Where(x => x.AccountTypeId == AccType.Income);
                var expenseHeadsQueryable = _context.AccountHeads.Where(x => x.AccountTypeId == AccType.Expense);
                var incomeHeads = _accountService.PrepareAccountHeadModelsWithBalance(incomeHeadsQueryable, requestModel.RootAccountHeadId, requestModel.Level, requestModel.LevelsToLoad, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);
                var expenseHeads = _accountService.PrepareAccountHeadModelsWithBalance(incomeHeadsQueryable, requestModel.RootAccountHeadId, requestModel.Level, requestModel.LevelsToLoad, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    //  _pdfService.PrintIncomeStatementReportToPdf(stream, _context.Branches.Find(requestModel.BranchId), list, "Income Statement ", requestModel.FromDate, requestModel.ToDate, income - expense);
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
    }
}
