using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.PermissionHelpers;
using Application.RequestModel;
using Application.Services;
using Application.ViewModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PbApp.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SupplierPaymentController : ControllerBase
    {
        private readonly IBaseQueryService<SupplierPayment, SupplierPaymentRequestModel, SupplierPaymentViewModel> _supplierPaymentQueryService;
        private readonly IBaseCommandService<SupplierPayment> _supplierPaymentCommandService;
        private readonly IBaseCommandService<Supplier> _supplierCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IPdfService _pdfService;
        private readonly GHP_POS_DBContext _context;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;
        public SupplierPaymentController(IBaseQueryService<SupplierPayment, SupplierPaymentRequestModel, SupplierPaymentViewModel> supplierPaymentQueryService,
            IBaseCommandService<SupplierPayment> supplierPaymentCommandService,
            IHttpContextAccessor contextAccessor,
            IBaseCommandService<Supplier> supplierCommandService,
            GHP_POS_DBContext context, IPdfService pdfService,
            ICommonService commonService,
            IConfiguration configuration)
        {
            _supplierPaymentCommandService=supplierPaymentCommandService;
            _supplierPaymentQueryService = supplierPaymentQueryService;
            _supplierCommandService = supplierCommandService;
            _contextAccessor = contextAccessor;
            _context = context;
            _pdfService = pdfService;
            _commonService = commonService;
            _configuration = configuration;
        }
        [Authorize(Policy = Permissions.SupplierPayments.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<SupplierPaymentViewModel>, int>>> Search(SupplierPaymentRequestModel request)
        {
            try
            {
                Tuple<List<SupplierPaymentViewModel>, int> content = await _supplierPaymentQueryService.SearchAsync(request);
                return await Result<Tuple<List<SupplierPaymentViewModel>, int>>.SuccessAsync(content, "SupplierPayments Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<SupplierPaymentViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(SupplierPaymentRequestModel request)
        {
            try
            {
                request.Page = -1;
                request.IsAscending = true;
                Tuple<List<SupplierPaymentViewModel>, int> content = await _supplierPaymentQueryService.SearchAsync(request);
                byte[] bytes;
               
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintSupplierPaymentReportToPdf(stream, content.Item1, "Supplier Payment Report", _context.Branches.Find(request.BranchId), request.fromDate, request.ToDate);
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
        [Authorize(Policy = Permissions.SupplierPayments.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<SupplierPaymentViewModel>> Detail(string id)
        {
            try
            {
                SupplierPaymentViewModel content = await _supplierPaymentQueryService.GetDetailAsync(id);
                return await Result<SupplierPaymentViewModel>.SuccessAsync(content, "SupplierPayment Found");
            }
            catch (Exception exception)
            {
                return await Result<SupplierPaymentViewModel>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        [Authorize(Policy = Permissions.SupplierPayments.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("bill-no")]
        [HttpGet]
        public IActionResult GetBillNo()
        {
            return Ok(new { BillNo = _supplierPaymentCommandService.GetReferanceId(character: "SP") });
        }
        [Authorize(Policy = Permissions.SupplierPayments.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] SupplierPaymentModel model)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var dbSupplier = await _context.Suppliers.FindAsync(model.SupplierId);
                    var count = _context.SupplierPayments.Count() + 1;
                    SupplierPayment payment = new SupplierPayment();
                    DateTime paymentDate;
                    var isValidDate = DateTime.TryParseExact(model.PaymentDate, "dd/MM/yyyy", null, DateTimeStyles.None, out paymentDate);
                    payment.Id = Guid.NewGuid().ToString();
                    payment.BillNo = "SP" + count.ToString().PadLeft(7, '0');
                    payment.BranchId = model.BranchId;
                    payment.SupplierId = model.SupplierId;
                    payment.PaymentDate = isValidDate ? paymentDate : DateTime.Now;
                    payment.PaymentMethodId = model.PaymentMethodId;
                    payment.PaymentAmount = model.PaymentAmount;
                    payment.Remark = model.Remark;
                    payment.SupplierPreviousBalance = dbSupplier.Balance;
                    payment.SupplierBalance = dbSupplier.Balance - model.PaymentAmount;
                    payment.CreatedOn = DateTime.Now;
                    payment.UpdatedOn = DateTime.Now;
                    payment.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    payment.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _context.SupplierPayments.AddAsync(payment);
                    

                   var isAccountsActive= _configuration.GetValue<bool>("AccountSettings:IsActive");

                    if (isAccountsActive)
                    {
                        //payment given........................................
                        var paymentFrom =await _context.Branches.FindAsync(payment.BranchId);
                        var paymentMethod = await _context.PaymentMethods.FindAsync(payment.PaymentMethodId);
                        if(paymentMethod==null) throw new Exception("Payment Method not Selected");
                            //cash in hand/bank a/c credit
                        await _commonService.HitAccount(_context, paymentFrom.Id, payment.BillNo, "Supplier Payment",
                               paymentMethod.AccountId, 0, payment.PaymentAmount,payment.PaymentDate);
                      
                         //supplier a/c debit
                        await _commonService.HitAccount(_context, paymentFrom.Id, payment.BillNo, "Supplier Payment",
                           dbSupplier.AccountId, payment.PaymentAmount, 0, payment.PaymentDate);
                       //payment receive........................................................
                        if (dbSupplier.IsBranchSupplier)
                        {
                            var paymentTo =  _context.Branches.Where(x => x.SupplierAccountId == dbSupplier.AccountId)?.FirstOrDefault();
                            
                            //cash in hand a/c debit
                            await _commonService.HitAccount(_context, paymentTo.Id, payment.BillNo, "Supplier Payment",
                               paymentMethod.AccountId, payment.PaymentAmount,0, payment.PaymentDate);
                            //destination branch supplier account credit
                            await _commonService.HitAccount(_context, paymentTo.Id, payment.BillNo, "Supplier Payment",
                            paymentFrom.SupplierAccountId,0, payment.PaymentAmount, payment.PaymentDate);
                        }
                    }
                    else
                    {
                        //adjust supplier balance
                        dbSupplier.Balance -= model.PaymentAmount;
                        _context.Suppliers.Update(dbSupplier);
                    }
                   

                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return await Result<string>.SuccessAsync(model.Id, "SupplierPayment Added Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return await Result<SupplierPayment>.FailAsync(exception.ToString(), exception.Message);
                }
            }
        }
        [Authorize(Policy = Permissions.SupplierPayments.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/supplierpayment/update")]
        public virtual async Task<Result> Put([FromBody] SupplierPayment model)
        {
            try
            {
                model.UpdatedOn = DateTime.UtcNow;
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _supplierPaymentCommandService.UpdateAsync(model);
                //adjust supplier balance
               // decimal diffNetAmount = model.PaymentAmount - purchase.NetAmount;
               /// var dbSupplier = await _supplierCommnadService.FindAsync(model.SupplierId);
               // dbSupplier.Balance += diffNetAmount;
             //   await _supplierCommnadService.UpdateAsync(dbSupplier);
                return await Result<SupplierPayment>.SuccessAsync(model, "SupplierPayment Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }

        }
        [Authorize(Policy = Permissions.SupplierPayments.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                SupplierPayment supplierPayment = await _supplierPaymentCommandService.FindAsync(id);
                if (supplierPayment != null)
                {
                    supplierPayment.Deleted = true;
                    supplierPayment.UpdatedOn = DateTime.UtcNow;
                    supplierPayment.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _supplierPaymentCommandService.UpdateAsync(supplierPayment);
                }
               
                return await Result<string>.SuccessAsync(id, "SupplierPayment Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpGet("/api/supplierPayment/voucher/{id}")]
        public IActionResult SupplierPaymentVoucher(string id)
        {
            var supplierPayment = _context.SupplierPayments.Find(id);
            if (supplierPayment == null) throw new NotFoundResultException("Not Found");
            var supplier = _context.Suppliers.Find(supplierPayment.SupplierId);
            var paymentMethod = _context.PaymentMethods.Find(supplierPayment.PaymentMethodId);
            var branch = _context.Branches.Find(supplierPayment.BranchId);
            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintSupplierPaymentVoucherToPdf(stream, supplier, branch, supplierPayment, paymentMethod);
                    bytes = stream.ToArray();
                    // bytes =_pdfService.AddFooterInfoForA5Page(bytes);
                }
                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exc)
            {
                throw;
            }

        }
    }
}
