using Application.Core.Entities;
using Application.ViewModel;

namespace Application.Services
{
    public interface IAccountService
    {
        decimal CalculateBalanceByAccountId(string accountId, string type, string fiscalYearId, DateTime? fromDate, DateTime? toDate, string? branchId = null);
        IList<ChartOfAccountModel> PrepareAccountHeadModels(string? rootAccountHeadId, int level, int levelsToLoad);
        IList<AccountHeadDropdownModel> PrepareAccountHeadWithParents();
        IList<AccountHeadDropdownModel> PrepareControlAccountHeadWithParents(string? branchId);
        string GetAccountHeadNameWithParents(AccountHead? accountHead);
        IList<AccountHeadWithBalanceViewModel> PrepareAccountHeadModelsWithBalance(IQueryable<AccountHead> queryable, string? rootAccountHeadId, int level, int levelsToLoad, string fiscalYearId, DateTime? fromDate, DateTime? toDate, string? branchId = null, decimal profitLoss = 0);
        IList<SubsidiaryLedgerViewModel> SubsidiaryLedger(string? accountId, DateTime? fromDate, DateTime? toDate, string? branchId);
        IList<TrialBalanceViewModel> TrialBalance(IQueryable<AccountHead> queryable, string? rootAccountHeadId, int level, int levelsToLoad, string fiscalYearId, DateTime? fromDate, DateTime? toDate, string? branchId = null);
        IList<AccountHead> GetAllSubAccountsByParentAccountId(string? parentAccountId, IQueryable<AccountHead> accountHeads, string? branchId = null);
        TrialBalanceViewModel PrepareTrialBalanceReportLine(AccountHead account, string fiscalYearId, DateTime? fromDate, DateTime? toDate, string? branchId = null);
        void PrepareNewFiscalYear(string newFiscalYearId, string branchId);
        IList<TransactionViewModel> CustomerTransaction(IQueryable<AccountHead> queryable, string fiscalYearId, DateTime? fromDate, DateTime? toDate, string? branchId = null);
        IList<TransactionViewModel> SupplierTransaction(IQueryable<AccountHead> queryable, string fiscalYearId, DateTime? fromDate, DateTime? toDate, string? branchId = null);

    }
}
