using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
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
    public class AccountHeadController : ControllerBase
    {
        private readonly IBaseQueryService<AccountHead, AccountHeadRequestModel, AccountHeadViewModel> _accountHeadQueryService;
        private readonly IBaseCommandService<AccountHead> _accountHeadCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly GHP_POS_DBContext _context;
        private readonly ICommonService _commonService;
        public AccountHeadController(IBaseQueryService<AccountHead, AccountHeadRequestModel, AccountHeadViewModel> accountHeadQueryService,
            IBaseCommandService<AccountHead> accountHeadCommandService,
        IHttpContextAccessor contextAccessor,
        GHP_POS_DBContext context,
        ICommonService commonService)
        {
            _accountHeadCommandService = accountHeadCommandService;
            _accountHeadQueryService = accountHeadQueryService;
            _contextAccessor = contextAccessor;
            _context = context;
            _commonService = commonService;
        }

        [HttpPost]
        [Authorize(Policy = Permissions.AccountHeads.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<AccountHeadViewModel>, int>>> Search(AccountHeadRequestModel request)
        {
            try
            {
                Tuple<List<AccountHeadViewModel>, int> content = await _accountHeadQueryService.SearchAsync(request);
                return await Result<Tuple<List<AccountHeadViewModel>, int>>.SuccessAsync(content, "AccountHeads Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<AccountHeadViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.AccountHeads.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<AccountHeadViewModel>> Detail(string id)
        {
            try
            {
                AccountHeadViewModel content = await _accountHeadQueryService.GetDetailAsync(id);
                return await Result<AccountHeadViewModel>.SuccessAsync(content, "AccountHead Found");
            }
            catch (Exception exception)
            {
                return await Result<AccountHeadViewModel>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        [Authorize(Policy = Permissions.AccountHeads.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] AccountHead model)
        {
            try
            {
                model.Id = Guid.NewGuid().ToString();
                model.Code =_commonService.GetAccountCode(model.AccountTypeId);
                model.Level =await _commonService.GetAccountLevel(model.ParentId);
                model.CreatedOn = DateTime.Now;
                model.UpdatedOn = DateTime.Now;
                model.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _accountHeadCommandService.AddAsync(model);
                return await Result<string>.SuccessAsync(model.Id , "AccountHead Added Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }

       
        [HttpGet()]
        [Route("code/{accountTypeId}")]
        public IActionResult GetReferenceNo(string accountTypeId)
        {
            return Ok(new {Code=_commonService.GetAccountCode(accountTypeId) });
        }
        [Authorize(Policy = Permissions.AccountHeads.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/accountHead/update")]
        public virtual async Task<Result> PostUpdate([FromBody] AccountHead model)
        {
            try
            {
                model.CreatedOn = DateTime.Now;
                model.UpdatedOn = DateTime.Now;
                model.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _accountHeadCommandService.UpdateAsync(model);
                return await Result<string>.SuccessAsync(model.Id, "AccountHead Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
          
        }
        [Authorize(Policy = Permissions.AccountHeads.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                AccountHead accountHead = await _accountHeadCommandService.FindAsync(id);
                if (accountHead != null)
                {
                    accountHead.Deleted = true;
                    accountHead.UpdatedOn = DateTime.Now;
                    accountHead.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _accountHeadCommandService.UpdateAsync(accountHead);
                }
                return await Result<string>.SuccessAsync(id, "AccountHead Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
      
    }
}

