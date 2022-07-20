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
    public class BranchController : ControllerBase
    {
        private readonly IBaseQueryService<Branch, BranchRequestModel, BranchViewModel> _branchQueryService;
        private readonly IBaseCommandService<Branch> _branchCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly GHP_POS_DBContext _context;
        private readonly ICommonService _commonService;
        public BranchController(IBaseQueryService<Branch, BranchRequestModel, BranchViewModel> branchQueryService,
            IBaseCommandService<Branch> branchCommandService,
        IHttpContextAccessor contextAccessor,
        GHP_POS_DBContext context,
            ICommonService commonService)
        {
            _branchCommandService = branchCommandService;
            _branchQueryService = branchQueryService;
            _contextAccessor = contextAccessor;
            _context = context;
            _commonService = commonService;
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Branches.ViewAll)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<BranchViewModel>, int>>> Search(BranchRequestModel request)
        {
            try
            {
                Tuple<List<BranchViewModel>, int> content = await _branchQueryService.SearchAsync(request);
                return await Result<Tuple<List<BranchViewModel>, int>>.SuccessAsync(content, "Branches Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<BranchViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Branches.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<BranchViewModel>> Detail(string id)
        {
            try
            {
                BranchViewModel content = await _branchQueryService.GetDetailAsync(id);
                return await Result<BranchViewModel>.SuccessAsync(content, "Branch Found");
            }
            catch (Exception exception)
            {
                return await Result<BranchViewModel>.FailAsync(exception.ToString(), exception.Message);
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
        [Authorize(Policy = Permissions.Branches.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] Branch model)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
            {
                    //save Account Head
                    var accountHead = new AccountHead
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = _commonService.GetAccountCode(AccType.Liability),
                        AccountTypeId = AccType.Liability,//liablility  id=2
                        Level = await _commonService.GetAccountLevel("15"),
                        Name = model.Name,
                        ParentId = "15",//parent Sundry Creditors from database "15"
                        IsControlAccount = true,
                        CreatedOn = DateTime.Now,
                        CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId(),
                        UpdatedOn = DateTime.Now,
                        UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId()
                    };
                    await _context.AccountHeads.AddAsync(accountHead);
                    await _context.SaveChangesAsync();
                    //branch
                    model.Id = Guid.NewGuid().ToString();
                model.Code = _branchCommandService.GetReferanceId("B");
                    model.SupplierAccountId = accountHead.Id;
                model.CreatedOn = DateTime.Now;
                model.UpdatedOn= DateTime.Now;
                model.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _context.Branches.AddAsync(model);
                    await _context.SaveChangesAsync();
                await dbContextTransaction.CommitAsync();
                return await Result<string>.SuccessAsync(model.Id, "Branch Added Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return await Result<string>.FailAsync(exception.ToString(), exception.Message);
                }
            }
        }

        [Authorize(Policy = Permissions.Users.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("code")]
        [HttpGet]
        public IActionResult GetReferenceNo()
        {
            string code = _branchCommandService.GetReferanceId("B");
            return Ok(new { Code = code });
        }
        [Authorize(Policy = Permissions.Branches.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/branch/update")]
        public virtual async Task<Result> PostUpdate([FromBody] Branch model)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {        //update Account Head
                    var accountHead = await _context.AccountHeads.FindAsync(model.SupplierAccountId);
                    accountHead.Name = model.Name;
                    _context.AccountHeads.Update(accountHead);
                    await _context.SaveChangesAsync();
                    //branch
                    model.UpdatedOn = DateTime.Now;
                    model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    _context.Branches.Update(model);
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return await Result<string>.SuccessAsync(model.Id, "Branch Added Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return await Result<string>.FailAsync(exception.ToString(), exception.Message);
                }
            }

        }
        [Authorize(Policy = Permissions.Branches.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                Branch branch = await _branchCommandService.FindAsync(id);
                if (branch != null)
                {
                    branch.Deleted = true;
                    branch.UpdatedOn = DateTime.Now;
                    branch.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _branchCommandService.UpdateAsync(branch);
                }
                return await Result<string>.SuccessAsync(id, "Branch Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}

