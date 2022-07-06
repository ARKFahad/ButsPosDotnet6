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
using PbApp.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodController : ControllerBase
    {
        private readonly IBaseQueryService<PaymentMethod, PaymentMethodRequestModel, PaymentMethodViewModel> _paymentMethodQueryService;
        private readonly IBaseCommandService<PaymentMethod> _paymentMethodCommnadService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly GHP_POS_DBContext _context;
        private readonly ICommonService _commonService;
        public PaymentMethodController(IBaseQueryService<PaymentMethod, PaymentMethodRequestModel, PaymentMethodViewModel> paymentMethodQueryService,
            IBaseCommandService<PaymentMethod> paymentMethodCommnadService,
            IHttpContextAccessor contextAccessor,
            GHP_POS_DBContext context,
            ICommonService commonService)
        {
            _paymentMethodCommnadService = paymentMethodCommnadService;
            _paymentMethodQueryService = paymentMethodQueryService;
            _contextAccessor = contextAccessor;
            _context = context;
            _commonService = commonService;
        }

        [HttpPost]
        [Authorize(Policy = Permissions.PaymentMethods.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<PaymentMethodViewModel>, int>>> Search(PaymentMethodRequestModel request)
        {
            try
            {
                Tuple<List<PaymentMethodViewModel>, int> content = await _paymentMethodQueryService.SearchAsync(request);
                return await Result<Tuple<List<PaymentMethodViewModel>, int>>.SuccessAsync(content, "Payment Methods Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<PaymentMethodViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.PaymentMethods.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<PaymentMethodViewModel>> Detail(string id)
        {
            try
            {
                PaymentMethodViewModel content = await _paymentMethodQueryService.GetDetailAsync(id);
                return await Result<PaymentMethodViewModel>.SuccessAsync(content, "PaymentMethod Found");
            }
            catch (Exception exception)
            {
                return await Result<PaymentMethodViewModel>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        // [Authorize(Policy = Permissions.Users.View)]
        //// [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // [Route("reference-no")]
        // [HttpGet]
        // public async Task<IActionResult> GetReferenceNo()
        // {
        //     string referenceNo = await _service.GetReferanceIdAsync();
        //     return Ok(new { ReferenceNo = referenceNo });
        // }
        [Authorize(Policy = Permissions.PaymentMethods.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] PaymentMethod model)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    //save Account Head
                    var accountHead = new AccountHead
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = _commonService.GetAccountCode(AccType.Asset),
                        AccountTypeId = AccType.Asset,
                        Level = await _commonService.GetAccountLevel("5"),
                        Name = model.Name,
                        ParentId = "5",//Cash At Bank Account Id=5
                        IsControlAccount = true,
                        CreatedOn = DateTime.Now,
                        CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId(),
                        UpdatedOn = DateTime.Now,
                        UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId()
                    };
                    await _context.AccountHeads.AddAsync(accountHead);
                    await _context.SaveChangesAsync();
                    //save payment method
                    model.Id = Guid.NewGuid().ToString();
                    model.AccountId = accountHead.Id;
                    model.CreatedOn = DateTime.Now;
                    model.UpdatedOn = DateTime.Now;
                    model.CreatedBy = _contextAccessor.HttpContext.User?.GetUserId();
                    model.UpdatedBy = _contextAccessor.HttpContext.User?.GetUserId();
                    await _context.PaymentMethods.AddAsync(model);
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return await Result<string>.SuccessAsync(model.Id, "Payment Method Added Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return await Result<string>.FailAsync(exception.ToString(), exception.Message);
                }
            }
        }
        [Authorize(Policy = Permissions.PaymentMethods.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/paymentmethod/update")]
        public virtual async Task<Result> Put([FromBody] PaymentMethod model)
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
                    //update Expense Type
                    model.UpdatedOn = DateTime.UtcNow;
                    model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    _context.PaymentMethods.Update(model);
                    await _context.SaveChangesAsync();

                    await dbContextTransaction.CommitAsync();
                    return await Result<string>.SuccessAsync(model.Id, "Payment Method Updated Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return await Result<string>.FailAsync(exception.ToString(), exception.Message);
                }
            }

        }
        [Authorize(Policy = Permissions.PaymentMethods.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                PaymentMethod paymentMethod = await _paymentMethodCommnadService.FindAsync(id);
                if (paymentMethod != null)
                {
                    paymentMethod.Deleted = true;
                    paymentMethod.UpdatedOn = DateTime.UtcNow;
                    paymentMethod.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _paymentMethodCommnadService.UpdateAsync(paymentMethod);
                }
                return await Result<string>.SuccessAsync(id, "PaymentMethod Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}

