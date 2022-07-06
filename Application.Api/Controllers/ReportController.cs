using Application.Core.Common;
using Application.Core.Data;
using Application.RequestModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Application.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Application.Core.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IO;
using Application.Services;
using Application.Core.ReportModels;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IPdfService _pdfService;
        public ReportController(GHP_POS_DBContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }
        [Authorize(Policy = Permissions.Reports.PL)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("pl")]
        public virtual async Task<Result<Tuple<List<PLModel>, int>>> Profit([FromBody] PLRequestModel request)
        {
            try
            {
                if (request.fromDate.HasValue)
                {
                    request.fromDate = request.fromDate.Value.AddDays(-1);
                }

                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@page",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.Page
                        },
                        new SqlParameter() {
                            ParameterName = "@rowsPerPage",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.RowsPerPage
                        },
                      new SqlParameter() {
                            ParameterName = "@totalCount",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Output,
                            Value = request.ToDate?? Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@branchId",
                            SqlDbType =  System.Data.SqlDbType.NVarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.BranchId?? Convert.DBNull
                        },
                        new SqlParameter() {
                            ParameterName = "@fromDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.fromDate?? Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@toDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.ToDate?? Convert.DBNull
                        }};
                var list = await _context.PLModels.FromSqlRaw("[dbo].[spProfitLoss] @page,@rowsPerPage, @totalCount OUTPUT, @branchId, @fromDate, @toDate", param).ToListAsync();
                Tuple<List<PLModel>, int> content = new Tuple<List<PLModel>, int>(list, (int)param[2].Value);
                return await Result<Tuple<List<PLModel>, int>>.SuccessAsync(content, "PL Found");
            }
            catch(Exception e)
            {
                return await Result<Tuple<List<PLModel>, int>>.FailAsync(e.ToString(), e.Message);
            }


        }
        [HttpPost]
        [Route("pl/print")]
        public virtual async Task<IActionResult> Print(PLRequestModel request)
        {
            request.IsAscending = true;

            try
            {
                DateTime? tempDate;
                request.RowsPerPage = int.MaxValue;
                if (request.fromDate.HasValue)
                {
                    request.fromDate = request.fromDate.Value.AddDays(-1);
                     tempDate = request.fromDate.GetValueOrDefault().AddDays(1);
                }
                else {
                    tempDate = null;
                }
                var param = new SqlParameter[] {
                        new SqlParameter() {
                            ParameterName = "@page",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.Page
                        },
                        new SqlParameter() {
                            ParameterName = "@rowsPerPage",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.RowsPerPage
                        },
                      new SqlParameter() {
                            ParameterName = "@totalCount",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Output,
                            Value = request.ToDate?? Convert.DBNull
                        },
                     new SqlParameter() {
                            ParameterName = "@branchId",
                            SqlDbType =  System.Data.SqlDbType.NVarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.BranchId?? Convert.DBNull
                        },
                        new SqlParameter() {
                            ParameterName = "@fromDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.fromDate?? Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@toDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.ToDate?? Convert.DBNull
                        }};
                var list = await _context.PLModels.FromSqlRaw("[dbo].[spProfitLoss] @page,@rowsPerPage, @totalCount OUTPUT, @branchId, @fromDate, @toDate", param).ToListAsync();

                byte[] bytes;
                if (!request.fromDate.HasValue)
                {
                    request.ToDate = null;
                }
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintProfitReportToPdf(stream, _context.Branches.Find(request.BranchId), list, "Profit Report ",tempDate,request.ToDate);
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

        [Authorize(Policy = Permissions.Reports.Balance)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("balance")]
        public virtual async Task<Result<Tuple<List<BalanceReportLine>, int>>> BalanceReport([FromBody] BalanceRequestModel request)
        {
            try
            {
                if (request.fromDate.HasValue)
                {
                    request.fromDate = request.fromDate.Value.AddDays(-1);
                }

                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@page",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.Page
                        },
                        new SqlParameter() {
                            ParameterName = "@rowsPerPage",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.RowsPerPage
                        },
                      new SqlParameter() {
                            ParameterName = "@totalCount",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Output,
                            Value = request.ToDate?? Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@branchId",
                            SqlDbType =  System.Data.SqlDbType.NVarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.BranchId?? Convert.DBNull
                        },
                        new SqlParameter() {
                            ParameterName = "@fromDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.fromDate?? Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@toDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.ToDate?? Convert.DBNull
                        }};
                var list = await _context.BalanceReportLines.FromSqlRaw("[dbo].[spBalance] @page,@rowsPerPage, @totalCount OUTPUT, @branchId, @fromDate, @toDate", param).ToListAsync();
                Tuple<List<BalanceReportLine>, int> content = new Tuple<List<BalanceReportLine>, int>(list, (int)param[2].Value);
                return await Result<Tuple<List<BalanceReportLine>, int>>.SuccessAsync(content, "Balance Found");
            }
            catch (Exception e)
            {
                return await Result<Tuple<List<BalanceReportLine>, int>>.FailAsync(e.ToString(), e.Message);
            }


        }
        [HttpPost]
        [Route("balance/print")]
        public virtual async Task<IActionResult> PrintBalanceReport(BalanceRequestModel request)
        {
            request.IsAscending = true;

            try
            {
                DateTime? tempDate;
                request.RowsPerPage = int.MaxValue;
                if (request.fromDate.HasValue)
                {
                    request.fromDate = request.fromDate.Value.AddDays(-1);
                    tempDate = request.fromDate.GetValueOrDefault().AddDays(1);
                }
                else
                {
                    tempDate = null;
                }
                var param = new SqlParameter[] {
                        new SqlParameter() {
                            ParameterName = "@page",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.Page
                        },
                        new SqlParameter() {
                            ParameterName = "@rowsPerPage",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.RowsPerPage
                        },
                      new SqlParameter() {
                            ParameterName = "@totalCount",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Output,
                            Value = request.ToDate?? Convert.DBNull
                        },
                     new SqlParameter() {
                            ParameterName = "@branchId",
                            SqlDbType =  System.Data.SqlDbType.NVarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.BranchId?? Convert.DBNull
                        },
                        new SqlParameter() {
                            ParameterName = "@fromDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.fromDate?? Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@toDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.ToDate?? Convert.DBNull
                        }};
                var list = await _context.BalanceReportLines.FromSqlRaw("[dbo].[spBalance] @page,@rowsPerPage, @totalCount OUTPUT, @branchId, @fromDate, @toDate", param).ToListAsync();

                byte[] bytes;
                if (!request.fromDate.HasValue)
                {
                    request.ToDate = null;
                }
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintBalanceReportToPdf(stream, _context.Branches.Find(request.BranchId), list, "Balance Report ", tempDate, request.ToDate);
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
        [Authorize(Policy = Permissions.Reports.SupplierLedger)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("supplier-ledger")]
        public virtual async Task<Result<Tuple<List<SupplierLedgerReportLine>, int>>> SupplierLedger([FromBody] SupplierLedgerRequestModel request)
        {
            try
            {

                if (request.FromDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.AddDays(-1);
                }

                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@page",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.Page
                        },
                        new SqlParameter() {
                            ParameterName = "@rowsPerPage",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.RowsPerPage
                        },
                      new SqlParameter() {
                            ParameterName = "@totalCount",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Output,
                            Value = request.ToDate?? Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@supplierId",
                            SqlDbType =  System.Data.SqlDbType.NVarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.SupplierId?? Convert.DBNull
                        },
                        new SqlParameter() {
                            ParameterName = "@fromDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.FromDate?? Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@toDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.ToDate?? Convert.DBNull
                        }};
                var list = await _context.SupplierLedgerReportLines.FromSqlRaw("[dbo].[spSupplierLedger] @page,@rowsPerPage, @totalCount OUTPUT, @supplierId, @fromDate, @toDate", param).ToListAsync();
                Tuple<List<SupplierLedgerReportLine>, int> content = new Tuple<List<SupplierLedgerReportLine>, int>(list, (int)param[2].Value);
                return await Result<Tuple<List<SupplierLedgerReportLine>, int>>.SuccessAsync(content, "Supplier Ledger Found");
            }
            catch (Exception e)
            {
                return await Result<Tuple<List<SupplierLedgerReportLine>, int>>.FailAsync(e.ToString(), e.Message);
            }


        }
        [HttpPost]
        [Route("supplier-ledger/print")]
        public virtual async Task<IActionResult> PrintSupplierLedger(SupplierLedgerRequestModel request)
        {

            request.IsAscending = true;
            try
            {
                request.RowsPerPage = int.MaxValue;
                if (request.FromDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.AddDays(-1);
                }
                var param = new SqlParameter[] {
                        new SqlParameter() {
                            ParameterName = "@page",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.Page
                        },
                        new SqlParameter() {
                            ParameterName = "@rowsPerPage",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.RowsPerPage
                        },
                      new SqlParameter() {
                            ParameterName = "@totalCount",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Output,
                            Value = request.ToDate?? Convert.DBNull
                        },
                       new SqlParameter() {
                            ParameterName = "@supplierId",
                            SqlDbType =  System.Data.SqlDbType.NVarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.SupplierId?? Convert.DBNull
                        },
                        new SqlParameter() {
                            ParameterName = "@fromDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.FromDate?? Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@toDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.ToDate?? Convert.DBNull
                        }};
                var list = await _context.SupplierLedgerReportLines.FromSqlRaw("[dbo].[spSupplierLedger] @page,@rowsPerPage, @totalCount OUTPUT, @supplierId, @fromDate, @toDate", param).ToListAsync();

                var prevBalance = _context.Suppliers.Find(request.SupplierId)?.OpeningBalance;
                if (request.FromDate.HasValue)
                {
                    var param1 = new SqlParameter[] {
                        new SqlParameter() {
                            ParameterName = "@page",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.Page
                        },
                        new SqlParameter() {
                            ParameterName = "@rowsPerPage",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.RowsPerPage
                        },
                      new SqlParameter() {
                            ParameterName = "@totalCount",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Output,
                            Value = request.ToDate?? Convert.DBNull
                        },
                       new SqlParameter() {
                            ParameterName = "@supplierId",
                            SqlDbType =  System.Data.SqlDbType.NVarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.SupplierId?? Convert.DBNull
                        },
                        new SqlParameter() {
                            ParameterName = "@fromDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@toDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.FromDate?? Convert.DBNull
                        }};
                   // request.ToDate = request.FromDate;
                    var supplierLedgerReportLines = await _context.SupplierLedgerReportLines.FromSqlRaw("[dbo].[spSupplierLedger] @page,@rowsPerPage, @totalCount OUTPUT, @supplierId, @fromDate, @toDate", param1).ToListAsync();
                    if (supplierLedgerReportLines.Any())
                    {
                        prevBalance = supplierLedgerReportLines.LastOrDefault()?.Balance;
                    }

                }


                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintSupplierLedgerReportToPdf(stream, _context.Suppliers.Find(request.SupplierId),prevBalance, list, "Supplier Ledger Report ", request.FromDate.GetValueOrDefault().AddDays(1), request.ToDate);
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

        [Authorize(Policy = Permissions.Reports.CustomerLedger)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("customer-ledger")]
        public virtual async Task<Result<Tuple<List<CustomerLedgerReportLine>, int>>> CustomerLedger([FromBody] CustomerLedgerRequestModel request)
        {
            try
            {
                if (request.fromDate.HasValue)
                {
                    request.fromDate = request.fromDate.Value.AddDays(-1);
                }

                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@page",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.Page
                        },
                        new SqlParameter() {
                            ParameterName = "@rowsPerPage",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.RowsPerPage
                        },
                      new SqlParameter() {
                            ParameterName = "@totalCount",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Output,
                            Value = request.ToDate?? Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@customerId",
                            SqlDbType =  System.Data.SqlDbType.NVarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.CustomerId?? Convert.DBNull
                        },
                        new SqlParameter() {
                            ParameterName = "@fromDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.fromDate?? Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@toDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.ToDate?? Convert.DBNull
                        }};
                var list = await _context.CustomerLedgerReportLines.FromSqlRaw("[dbo].[spCustomerLedger] @page,@rowsPerPage, @totalCount OUTPUT, @customerId, @fromDate, @toDate", param).ToListAsync();
                Tuple<List<CustomerLedgerReportLine>, int> content = new Tuple<List<CustomerLedgerReportLine>, int>(list, (int)param[2].Value);
                return await Result<Tuple<List<CustomerLedgerReportLine>, int>>.SuccessAsync(content, "Supplier Ledger Found");
            }
            catch (Exception e)
            {
                return await Result<Tuple<List<CustomerLedgerReportLine>, int>>.FailAsync(e.ToString(), e.Message);
            }


        }
        [HttpPost]
        [Route("customer-ledger/print")]
        public virtual async Task<IActionResult> PrintCustomerLedger(CustomerLedgerRequestModel request)
        {
            request.IsAscending = true;
            try
            {
                request.RowsPerPage = int.MaxValue;
                if (request.fromDate.HasValue)
                {
                    request.fromDate = request.fromDate.Value.AddDays(-1);
                }
                var param = new SqlParameter[] {
                        new SqlParameter() {
                            ParameterName = "@page",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.Page
                        },
                        new SqlParameter() {
                            ParameterName = "@rowsPerPage",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.RowsPerPage
                        },
                      new SqlParameter() {
                            ParameterName = "@totalCount",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Output,
                            Value = request.ToDate?? Convert.DBNull
                        },
                       new SqlParameter() {
                            ParameterName = "@customerId",
                            SqlDbType =  System.Data.SqlDbType.NVarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.CustomerId?? Convert.DBNull
                        },
                        new SqlParameter() {
                            ParameterName = "@fromDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.fromDate?? Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@toDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.ToDate?? Convert.DBNull
                        }};
                var list = await _context.CustomerLedgerReportLines.FromSqlRaw("[dbo].[spCustomerLedger] @page,@rowsPerPage, @totalCount OUTPUT, @customerId, @fromDate, @toDate", param).ToListAsync();
                var prevBalance = _context.Customers.Find(request.CustomerId)?.OpeningBalance;
                if (request.fromDate.HasValue)
                {
                    var param1 = new SqlParameter[] {
                        new SqlParameter() {
                            ParameterName = "@page",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.Page
                        },
                        new SqlParameter() {
                            ParameterName = "@rowsPerPage",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.RowsPerPage
                        },
                      new SqlParameter() {
                            ParameterName = "@totalCount",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Output,
                            Value = request.ToDate?? Convert.DBNull
                        },
                       new SqlParameter() {
                            ParameterName = "@customerId",
                            SqlDbType =  System.Data.SqlDbType.NVarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.CustomerId?? Convert.DBNull
                        },
                        new SqlParameter() {
                            ParameterName = "@fromDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value =  Convert.DBNull
                        },
                      new SqlParameter() {
                            ParameterName = "@toDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime2,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = request.fromDate?? Convert.DBNull
                        }};
                    var customerLedgerReportLines = await _context.CustomerLedgerReportLines.FromSqlRaw("[dbo].[spCustomerLedger] @page,@rowsPerPage, @totalCount OUTPUT, @customerId, @fromDate, @toDate", param1).ToListAsync();
                    if (customerLedgerReportLines.Any())
                    {
                        prevBalance = customerLedgerReportLines.LastOrDefault()?.Balance;
                    }

                }

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    //  _pdfService.PrintProfitReportToPdf(stream, _context.Customers.Find(request.SupplierId), list, "Customer Ledger Report ", request.fromDate, request.ToDate);
                    _pdfService.PrintCustomerLedgerReportToPdf(stream, _context.Customers.Find(request.CustomerId), prevBalance, list, "Customer Ledger Report ", request.fromDate.GetValueOrDefault().AddDays(1), request.ToDate);
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
