using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.RequestModel;
using Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IPdfService _pdfService;
        private readonly IAccountService _accountService;

        public TransactionController(GHP_POS_DBContext context, IPdfService pdfService, IAccountService accountService)
        {
            _context = context;
            _pdfService = pdfService;
            _accountService = accountService;
        }

        [Authorize(Policy = Permissions.TransactionReports.CustomerTransaction_SupplierTransaction)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("customer-transaction-print")]
        public IActionResult PrintCustomerTransactionReport(TransactionReportRequestModel requestModel)
        {
            try
            {
                requestModel.FiscalYearId = _context.FiscalYears.FirstOrDefault(x => x.IsActive)!.Id;
                var accountHeadsQueryable = _context.AccountHeads.AsQueryable();
                var customerTransactionViewModels = _accountService.CustomerTransaction(accountHeadsQueryable, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintCustomerTransactionReportToPdf(stream, _context.Branches.Find(requestModel.BranchId), customerTransactionViewModels, "Customer Balance Report", requestModel.FromDate, requestModel.ToDate);
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

        [Authorize(Policy = Permissions.TransactionReports.CustomerTransaction_SupplierTransaction)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("supplier-transaction-print")]
        public IActionResult PrintSupplierTransactionReport(TransactionReportRequestModel requestModel)
        {
            try
            {
                requestModel.FiscalYearId = _context.FiscalYears.FirstOrDefault(x => x.IsActive)!.Id;
                var accountHeadsQueryable = _context.AccountHeads.AsQueryable();
                var customerTransactionViewModels = _accountService.SupplierTransaction(accountHeadsQueryable, requestModel.FiscalYearId, requestModel.FromDate, requestModel.ToDate, requestModel.BranchId);

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintSupplierTransactionReportToPdf(stream, _context.Branches.Find(requestModel.BranchId), customerTransactionViewModels, "Supplier Balance Report", requestModel.FromDate, requestModel.ToDate);
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
