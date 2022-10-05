using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.ReportModels;
using Application.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
   public interface IPdfService
    {
        void PrintItemWisePurchaseReportToPdf(Stream stream, Branch branch, IList<ItemPurchaseViewModel> saleReportLines, string reportName, DateTime? fromDate, DateTime? toDate);
        void PrintItemWiseSaleProfitLossReportToPdf(Stream stream, Branch branch, IList<ItemSaleProfitLossViewModel> saleReportLines, string reportName, DateTime? fromDate, DateTime? toDate);
        void PrintSubsidiaryLedgerReportToPdf(MemoryStream stream, Branch? branch, IList<SubsidiaryLedgerViewModel> list, string reportName, DateTime? fromDate, DateTime? toDate);
        void PrintIncomeStatementReportToPdf(Stream stream, Branch? branch, IList<AccountHeadWithBalanceViewModel> incomeStatementReportLines, string reportName, DateTime? fromDate, DateTime? toDate,decimal netIncome=0);
        void PrintBalanceSheetReportToPdf(Stream stream, Branch? branch, IList<AccountHeadWithBalanceViewModel> balanceSheetReportLines, string reportName, DateTime? fromDate, DateTime? toDate,decimal netIncome=0);
        void PrintTrailBalanceReportToPdf(Stream stream, Branch? branch, IList<TrialBalanceViewModel> trialBalanceReportLines, string reportName, DateTime? fromDate, DateTime? toDate);
        void PrintBalanceReportToPdf(Stream stream, Branch? branch, IList<BalanceReportLine> balanceReportLines, string reportName, DateTime? fromDate, DateTime? toDate);
        void PrintProfitReportToPdf(Stream stream, Branch? branch, IList<PLModel> profitReportLines, string reportName, DateTime? fromDate, DateTime? toDatde);
        void PrintSupplierLedgerReportToPdf(Stream stream, Supplier? supplier, decimal? prevBalance, IList<SupplierLedgerReportLine> supplierLedgerReportLines, string reportName, DateTime? fromDate, DateTime? toDatde);
        void PrintCustomerLedgerReportToPdf(Stream stream, Customer? customer,decimal? prevBalance, IList<CustomerLedgerReportLine> customerLedgerReportLines, string reportName, DateTime? fromDate, DateTime? toDatde);
        void PrintStockReportToPdf(Stream stream, Branch? branch, IList<StockModel> stockReportLines, string reportName);
        void PrintOutOfStockReportToPdf(Stream stream, Branch? branch, IList<StockModel> stockReportLines, string reportName);
        void PrintPurchaseReportToPdf(Stream stream, Branch? branch, IList<PurchaseViewModel> purchaseReportLines, string reportName, DateTime? fromDate, DateTime? toDatde);
        void PrintSaleReportToPdf(Stream stream, Branch? branch, IList<SaleViewModel> saleReportLines, string reportName, DateTime? fromDate, DateTime? toDatde);
        void PrintExpenseReportToPdf(Stream stream, Branch? branch, IList<ExpenseViewModel> expenseReportLines, string reportName, DateTime? fromDate, DateTime? toDatde);
       
        void PrintSaleInvoiceToPdf(Stream stream, Customer? customer, Branch branch, Sale sale, List<SaleItemModel> saleItems);
        void PrintSupplierBalanceReportToPdf(Stream stream, List<SupplierViewModel> suppliers, String message);
        void PrintSupplierPaymentReportToPdf(Stream stream, List<SupplierPaymentViewModel> supplierpayments, String message,Branch branch,DateTime? fromdate,DateTime? toDate);
        void PrintCustomerPaymentReportToPdf(Stream stream, List<CustomerPaymentViewModel> customerpayments, String message, Branch branch, DateTime? fromdate, DateTime? toDate);
        //PrintCustomerPaymentReportToPdf
        void PrintCustomerBalanceReportToPdf(Stream stream, List<CustomerViewModel> customer, String message);
        void PrintSaleReturnReportToPdf(MemoryStream stream, List<SaleReturnViewModel> item1, string v, Branch branch, DateTime? fromDate, DateTime? toDate);
        byte[] AddFooterInfoForA5Page(byte[] bytes);
        byte[] AddPageNumbers(byte[] pdf);
        void PrintPurchaseInvoiceToPdf(MemoryStream stream, Supplier? supplier, Branch? branch, Purchase purchase, List<PurchaseItemModel> purchaseItems);
        void PrintSupplierPaymentVoucherToPdf(MemoryStream stream, Supplier? supplier, Branch? branch, SupplierPayment supplierPayment, PaymentMethod? paymentMethod);
        void PrintCustomerPaymentVoucherToPdf(MemoryStream stream, Customer? customer, Branch? branch, CustomerPayment customerPayment, PaymentMethod? paymentMethod);
        void PrintExpenseVoucherToPdf(MemoryStream stream, ExpenseType? expenseType, Branch? branch, Expense expense, PaymentMethod? paymentMethod);
        void PrintPayReceiveVoucherToPdf(MemoryStream stream, Branch? branch, PayReceive payReceive, AccountHead? accountHead, PaymentMethod? paymentMethod);
    }
}
