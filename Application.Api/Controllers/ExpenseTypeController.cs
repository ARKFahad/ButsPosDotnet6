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
    public class ExpenseTypeController : ControllerBase
    {
        private readonly IBaseQueryService<ExpenseType, ExpenseTypeRequestModel, ExpenseTypeViewModel> _expensetypeQueryService;
        private readonly IBaseCommandService<ExpenseType> _expensetypeCommnadService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly GHP_POS_DBContext _context;
        private readonly ICommonService _commonService;
        public ExpenseTypeController(IBaseQueryService<ExpenseType, ExpenseTypeRequestModel, ExpenseTypeViewModel> expensetypeQueryService,
            IBaseCommandService<ExpenseType> expensetypeCommnadService,
            IHttpContextAccessor contextAccessor,
            GHP_POS_DBContext context,
            ICommonService commonService)
        {
            _expensetypeCommnadService = expensetypeCommnadService;
            _expensetypeQueryService = expensetypeQueryService;
            _contextAccessor = contextAccessor;
            _context = context;
            _commonService = commonService;
        }

        [HttpPost]
        [Authorize(Policy = Permissions.ExpenseTypes.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<ExpenseTypeViewModel>, int>>> Search(ExpenseTypeRequestModel request)
        {
            try
            {
                Tuple<List<ExpenseTypeViewModel>, int> content = await _expensetypeQueryService.SearchAsync(request);
                return await Result<Tuple<List<ExpenseTypeViewModel>, int>>.SuccessAsync(content, "ExpenseType Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<ExpenseTypeViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.ExpenseTypes.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<ExpenseTypeViewModel>> Detail(string id)
        {
            try
            {
                ExpenseTypeViewModel content = await _expensetypeQueryService.GetDetailAsync(id);
                return await Result<ExpenseTypeViewModel>.SuccessAsync(content, "ExpenseType Found");
            }
            catch (Exception exception)
            {
                return await Result<ExpenseTypeViewModel>.FailAsync(exception.ToString(), exception.Message);
            }
        }

       
        [Authorize(Policy = Permissions.ExpenseTypes.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] ExpenseType model)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
            {
                    //save Account Head
                    var accountHead = new AccountHead
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = _commonService.GetAccountCode(AccType.Expense),
                        AccountTypeId = AccType.Expense,
                        Level = await _commonService.GetAccountLevel("24"),
                        Name = model.Name,
                        ParentId = "24",//parent Administrative Expenses from database "24"
                        IsControlAccount = true,
                        CreatedOn = DateTime.Now,
                        CreatedBy = _contextAccessor.HttpContext.User?.GetUserId(),
                        UpdatedOn = DateTime.Now,
                        UpdatedBy = _contextAccessor.HttpContext.User?.GetUserId()
                    };
                    await _context.AccountHeads.AddAsync(accountHead);
                    await _context.SaveChangesAsync();
                    //save supplier
                    model.Id = Guid.NewGuid().ToString();
                    model.AccountId = accountHead.Id;
                    model.CreatedOn = DateTime.UtcNow;
                    model.UpdatedOn = DateTime.UtcNow;
                    model.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _context.ExpenseTypes.AddAsync(model);
                    await _context.SaveChangesAsync();

                    await dbContextTransaction.CommitAsync();
                return await Result<string>.SuccessAsync(model.Id, "Expense Type Added Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return await Result<string>.FailAsync(exception.ToString(), exception.Message);
                }
            }
        }
        [Authorize(Policy = Permissions.ExpenseTypes.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/expensetype/update")]
        public virtual async Task<Result> Put([FromBody] ExpenseType model)
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
                    _context.ExpenseTypes.Update(model);
                    await _context.SaveChangesAsync();

                    await dbContextTransaction.CommitAsync();
                return await Result<string>.SuccessAsync(model.Id, "Expense Type Updated Successfully");
            }
            catch (Exception exception)
            {
                await dbContextTransaction.RollbackAsync();
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }

    }
        [Authorize(Policy = Permissions.ExpenseTypes.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                ExpenseType expenseType = await _expensetypeCommnadService.FindAsync(id);
                if (expenseType != null)
                {
                    expenseType.Deleted = true;
                    expenseType.UpdatedOn = DateTime.UtcNow;
                    expenseType.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _expensetypeCommnadService.UpdateAsync(expenseType);
                }
                return await Result<string>.SuccessAsync(id, "ExpenseType Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}

