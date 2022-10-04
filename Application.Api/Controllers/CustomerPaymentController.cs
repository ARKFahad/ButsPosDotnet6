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
    public class CustomerPaymentController : ControllerBase
    {
        private readonly IBaseQueryService<CustomerPayment, CustomerPaymentRequestModel, CustomerPaymentViewModel> _customerPaymentQueryService;
        private readonly IBaseCommandService<CustomerPayment> _customerPaymentCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IBaseCommandService<Customer> _customerCommandService;
        private readonly GHP_POS_DBContext _context;
        private readonly IPdfService _pdfService;
        private readonly IConfiguration _configuration;
        private readonly ICommonService _commonService;
        public CustomerPaymentController(IBaseQueryService<CustomerPayment, CustomerPaymentRequestModel, CustomerPaymentViewModel> customerPaymentQueryService,
            IBaseCommandService<CustomerPayment> customerPaymentCommandService,
            IHttpContextAccessor contextAccessor, IBaseCommandService<Customer> customerCommandService,
            GHP_POS_DBContext context, IPdfService pdfService,
            IConfiguration configuration,
            ICommonService commonService)
        {
            _customerPaymentCommandService=customerPaymentCommandService;
            _customerPaymentQueryService = customerPaymentQueryService;
            _contextAccessor = contextAccessor;
            _customerCommandService = customerCommandService;
            _context = context;
            _pdfService = pdfService;
            _configuration = configuration;
            _commonService = commonService;
        }
        [Authorize(Policy = Permissions.CustomerPayments.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<CustomerPaymentViewModel>, int>>> Search(CustomerPaymentRequestModel request)
        {
            try
            {
                Tuple<List<CustomerPaymentViewModel>, int> content = await _customerPaymentQueryService.SearchAsync(request);
                return await Result<Tuple<List<CustomerPaymentViewModel>, int>>.SuccessAsync(content, "CustomerPayments Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<CustomerPaymentViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(CustomerPaymentRequestModel request)
        {
            try
            {
                request.Page = -1;
                request.IsAscending = true;
                Tuple<List<CustomerPaymentViewModel>, int> content = await _customerPaymentQueryService.SearchAsync(request);
                byte[] bytes;

                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintCustomerPaymentReportToPdf(stream, content.Item1, "Customer Payment Report ", _context.Branches.Find(request.BranchId), request.fromDate, request.ToDate);
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
        [Authorize(Policy = Permissions.CustomerPayments.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<CustomerPaymentViewModel>> Detail(string id)
        {
            try
            {
                CustomerPaymentViewModel content = await _customerPaymentQueryService.GetDetailAsync(id);
                return await Result<CustomerPaymentViewModel>.SuccessAsync(content, "CustomerPayment Found");
            }
            catch (Exception exception)
            {
                return await Result<CustomerPaymentViewModel>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        [Authorize(Policy = Permissions.CustomerPayments.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("bill-no")]
        [HttpGet]
        public IActionResult GetReferenceNo()
        {
            return Ok(new { BillNo = _customerPaymentCommandService.GetReferanceId(character: "CP") });
        }
        [Authorize(Policy = Permissions.CustomerPayments.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] CustomerPaymentModel model)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var dbCustomer = await _context.Customers.FindAsync(model.CustomerId);
                    var count = _context.CustomerPayments.Count() + 1;
                    CustomerPayment payment = new CustomerPayment();
                    DateTime paymentDate;
                    var isValidDate = DateTime.TryParseExact(model.PaymentDate, "dd/MM/yyyy", null, DateTimeStyles.None, out paymentDate);
                    payment.Id = Guid.NewGuid().ToString();
                    payment.BillNo = "CP" + count.ToString().PadLeft(7, '0');
                    payment.BranchId = model.BranchId;
                    payment.CustomerId = model.CustomerId;
                    payment.PaymentDate = isValidDate ? paymentDate : DateTime.Now;
                    payment.PaymentMethodId = model.PaymentMethodId;
                    payment.PaymentAmount = model.PaymentAmount;
                    payment.Remark = model.Remark;
                    payment.CustomerPreviousBalance =dbCustomer.Balance;
                    payment.CustomerBalance= dbCustomer.Balance - model.PaymentAmount;
                    payment.CreatedOn = DateTime.Now;
                    payment.UpdatedOn = DateTime.Now;
                    payment.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    payment.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _context.CustomerPayments.AddAsync(payment);
                 
                    var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
                    if (isAccountsActive)
                    {
                        var paymentMethod = await _context.PaymentMethods.FindAsync(payment.PaymentMethodId);
                        if (paymentMethod == null) throw new Exception("Payment Method not Selected");
                       
                            //cash in hand/bank a/c debit
                            await _commonService.HitAccount(_context, payment.BranchId, payment.BillNo, "Customer Due Collection",
                               paymentMethod.AccountId, payment.PaymentAmount,0, payment.PaymentDate);
                        

                        //customer a/c credit
                            await _commonService.HitAccount(_context, payment.BranchId, payment.BillNo, "Customer Due Collection",
                              dbCustomer.AccountId,0, payment.PaymentAmount, payment.PaymentDate);
                    }
                    else
                    {
                        //adjust customer balance
                        dbCustomer.Balance -= model.PaymentAmount;
                        _context.Customers.Update(dbCustomer);
                    }

                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return await Result<string>.SuccessAsync(model.Id, "CustomerPayment Added Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return await Result<CustomerPayment>.FailAsync(exception.ToString(), exception.Message);
                }
            }
        }
        [Authorize(Policy = Permissions.CustomerPayments.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/customerpayment/update")]
        public virtual async Task<Result> Put([FromBody] CustomerPayment model)
        {
            try
            {
                model.UpdatedOn = DateTime.UtcNow;
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _customerPaymentCommandService.UpdateAsync(model);
                return await Result<CustomerPayment>.SuccessAsync(model, "CustomerPayment Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<CustomerPayment>.FailAsync(exception.ToString(), exception.Message);
            }

        }
        [Authorize(Policy = Permissions.CustomerPayments.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                CustomerPayment customerPayment = await _customerPaymentCommandService.FindAsync(id);
                if (customerPayment != null)
                {
                    customerPayment.Deleted = true;
                    customerPayment.UpdatedOn = DateTime.UtcNow;
                    customerPayment.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _customerPaymentCommandService.UpdateAsync(customerPayment);
                }
               
                return await Result<string>.SuccessAsync(id, "CustomerPayment Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpGet("/api/customerPayment/voucher/{id}")]
        public IActionResult CustomerPaymentVoucher(string id)
        {
            var customerPayment = _context.CustomerPayments.Find(id);
            if (customerPayment == null) throw new NotFoundResultException("Not Found");
            var customer = _context.Customers.Find(customerPayment.CustomerId);
            var paymentMethod = _context.PaymentMethods.Find(customerPayment.PaymentMethodId);
            var branch = _context.Branches.Find(customerPayment.BranchId);
            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintCustomerPaymentVoucherToPdf(stream, customer, branch, customerPayment, paymentMethod);
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
