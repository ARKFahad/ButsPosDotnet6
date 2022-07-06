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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AccountTypeController : ControllerBase
    {
        private readonly IBaseQueryService<AccountType, AccountTypeRequestModel, AccountTypeViewModel> _accountTypeQueryService;
        private readonly IBaseCommandService<AccountType> _accountTypeCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly GHP_POS_DBContext _context;
        private readonly IPdfService _pdfService;
        public AccountTypeController(IBaseQueryService<AccountType, AccountTypeRequestModel, AccountTypeViewModel> accountTypeQueryService,
            IBaseCommandService<AccountType> accountTypeCommandService, GHP_POS_DBContext context,
            IHttpContextAccessor contextAccessor, IPdfService pdfService)
        {
            _accountTypeCommandService = accountTypeCommandService;
            _accountTypeQueryService = accountTypeQueryService;
            _contextAccessor = contextAccessor;
            _context = context;
            _pdfService = pdfService;
        }
      //  [Authorize(Policy = Permissions.AccountTypes.View)]
       // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<AccountTypeViewModel>, int>>> Search(AccountTypeRequestModel request)
        {
            try
            {
               
                Tuple<List<AccountTypeViewModel>, int> content = await _accountTypeQueryService.SearchAsync(request);
                return await Result<Tuple<List<AccountTypeViewModel>, int>>.SuccessAsync(content, "AccountType Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<AccountTypeViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
     //   [Authorize(Policy = Permissions.AccountTypes.View)]
      //  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<AccountTypeViewModel>> Detail(string id)
        {
            try
            {
                AccountTypeViewModel content = await _accountTypeQueryService.GetDetailAsync(id);
                return await Result<AccountTypeViewModel>.SuccessAsync(content, "AccountType Found");
            }
            catch (Exception exception)
            {
                return await Result<AccountTypeViewModel>.FailAsync(exception.ToString(), exception.Message);
            }
        }


        //  [Authorize(Policy = Permissions.AccountTypes.Add)]
        // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] AccountType model)
        {
            try
            {
                model.Id = Guid.NewGuid().ToString();
               
                await _accountTypeCommandService.AddAsync(model);
                return await Result<AccountType>.SuccessAsync(model, "AccountType Added Successfully");
            }
            catch (Exception exception)
            {
                return await Result<AccountType>.FailAsync(exception.ToString(), exception.Message);
            }
        }
       // [Authorize(Policy = Permissions.AccountTypes.Update)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/AccountType/update")]
        public virtual async Task<Result> Put([FromBody] AccountType model)
        {
            try
            {
                await _accountTypeCommandService.UpdateAsync(model);
                return await Result<AccountType>.SuccessAsync(model, "AccountType Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<AccountType>.FailAsync(exception.ToString(), exception.Message);
            }

        }
       
      //  [Authorize(Policy = Permissions.AccountTypes.Remove)]
       // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                AccountType accountType = await _accountTypeCommandService.FindAsync(id);
                if (accountType != null)
                {
                    accountType.Deleted = true;
                    await _accountTypeCommandService.UpdateAsync(accountType);
                }
                return await Result<string>.SuccessAsync(id, "AccountType Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}
