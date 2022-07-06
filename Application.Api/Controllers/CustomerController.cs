using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Enums;
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IBaseQueryService<Customer, CustomerRequestModel, CustomerViewModel> _customerQueryService;
        private readonly IBaseCommandService<Customer> _customerCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IPdfService _pdfService;
        private readonly GHP_POS_DBContext _context;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;
        public CustomerController(IBaseQueryService<Customer, CustomerRequestModel, CustomerViewModel> customerQueryService,
            IBaseCommandService<Customer> customerCommandService,
            IHttpContextAccessor contextAccessor, IPdfService pdfService,
            GHP_POS_DBContext context,
            ICommonService commonService,
            IConfiguration configuration)
        {
            _customerCommandService = customerCommandService;
            _customerQueryService = customerQueryService;
            _contextAccessor = contextAccessor;
            _context = context;
            _pdfService = pdfService;
            _commonService = commonService;
            _configuration = configuration;
        }
        [Authorize(Policy = Permissions.Customers.ViewAll)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<CustomerViewModel>, int>>> Search(CustomerRequestModel request)
        {
            try
            {
                Tuple<List<CustomerViewModel>, int> content = await _customerQueryService.SearchAsync(request);
                return await Result<Tuple<List<CustomerViewModel>, int>>.SuccessAsync(content, "Customers Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<CustomerViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Customers.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<CustomerViewModel>> Detail(string id)
        {
            try
            {
                CustomerViewModel content = await _customerQueryService.GetDetailAsync(id);
                content.IsAccountActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
                return await Result<CustomerViewModel>.SuccessAsync(content, "Customer Found");
            }
            catch (Exception exception)
            {
                return await Result<CustomerViewModel>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(CustomerRequestModel request)
        {
            try
            {
                request.Page = -1;
                request.IsAscending = true;
                Tuple<List<CustomerViewModel>, int> content = await _customerQueryService.SearchAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintCustomerBalanceReportToPdf(stream, content.Item1, "Customer Balance Report ");
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

       
        [Route("code")]
        [HttpGet]
        public IActionResult GetReferenceNo()
        {
            string code = _customerCommandService.GetReferanceId("C");
            return Ok(new { Code = code });
        }
        [Authorize(Policy = Permissions.Customers.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] Customer model)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
            {
                    var branchId = _contextAccessor.HttpContext?.User?.GetBranchId();
                    //save Account Head
                    var accountHead = new AccountHead
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = _commonService.GetAccountCode(AccType.Asset),
                        AccountTypeId = AccType.Asset,
                        Level = await _commonService.GetAccountLevel("7b6ab461-4a10-4bbc-94a6-13d73ddfd4d4"),
                        Name = model.Name,
                        ParentId = "7b6ab461-4a10-4bbc-94a6-13d73ddfd4d4",//parent Debtors from database "7b6ab461-4a10-4bbc-94a6-13d73ddfd4d4"
                        IsControlAccount = true,
                        CreatedOn = DateTime.Now,
                        CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId(),
                        UpdatedOn = DateTime.Now,
                        UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId()
                    };
                    if (string.IsNullOrWhiteSpace(model.BranchId))
                        accountHead.BranchId = branchId;
                    else
                        accountHead.BranchId = model.BranchId;

                    await _context.AccountHeads.AddAsync(accountHead);
                    await _context.SaveChangesAsync();
                    //save customer
                 
                    
                    var count = _context.Customers.Count() + 1;
                    model.Id = Guid.NewGuid().ToString();
                    model.Code = "C" + count.ToString().PadLeft(7, '0');
                    model.AccountId = accountHead.Id;
                    model.Balance = model.OpeningBalance;
                    model.CreatedOn = DateTime.Now;
                    model.UpdatedOn = DateTime.Now;
                    model.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    if (string.IsNullOrWhiteSpace(model.BranchId))
                        model.BranchId = branchId;
                    await _context.Customers.AddAsync(model);


                    //var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
                    //if (isAccountsActive)
                    //{
                    //    if (model.OpeningBalance > 0)
                    //    {
                    //        var accountEntry = new AccountEntry
                    //        {
                    //            Id = Guid.NewGuid().ToString(),
                    //            FiscalYearId = _context.FiscalYears.Where(x => x.IsActive)?.FirstOrDefault()?.Id,
                    //            BranchId = model.BranchId,
                    //            Vnumber = "opening" + model.Code,
                    //            Description = "Customer Opening Balance",
                    //            AccountId = model.AccountId,
                    //            Debit = model.OpeningBalance,
                    //            Credit = 0,
                    //            IsOpeningBalance = true,
                    //            TransactionDate = DateTime.Now,
                    //            CreatedBy = _contextAccessor.HttpContext.User?.GetUserId()
                    //        };
                    //        await _context.AccountEntries.AddAsync(accountEntry);

                    //    }

                    //}
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return await Result<string>.SuccessAsync(model.Id, "Customer Added Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return await Result<string>.FailAsync(exception.ToString(), exception.Message);
                }
            }
        }
        [Authorize(Policy = Permissions.Customers.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/customer/update")]
        public virtual async Task<Result> Put([FromBody] Customer model)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
            {
                    //update Account Head
                    var accountHead = await _context.AccountHeads.FindAsync(model.AccountId);
                    accountHead.Name = model.Name;
                    _context.AccountHeads.Update(accountHead);
                    await _context.SaveChangesAsync();
                    //update customer
                    model.UpdatedOn = DateTime.Now;
                    model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    _context.Customers.Update(model);
                    await _context.SaveChangesAsync();

                    await dbContextTransaction.CommitAsync();
                    return await Result<string>.SuccessAsync(model.Id, "Customer Updated Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return await Result<string>.FailAsync(exception.ToString(), exception.Message);
                }
            }

        }
        [Authorize(Policy = Permissions.Customers.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                Customer customer = await _customerCommandService.FindAsync(id);
                if (customer != null)
                {
                    customer.Deleted = true;
                    customer.UpdatedOn = DateTime.UtcNow;
                    customer.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _customerCommandService.UpdateAsync(customer);
                }
                return await Result<string>.SuccessAsync(id, "Customer Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}
