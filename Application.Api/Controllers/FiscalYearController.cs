using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.PermissionHelpers;
using Application.RequestModel;
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
    public class FiscalYearController : ControllerBase
    {
        private readonly IBaseQueryService<FiscalYear, FiscalYearRequestModel, FiscalYearViewModel> _fiscalYearQueryService;
        private readonly IBaseCommandService<FiscalYear> _fiscalYearCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        public FiscalYearController(IBaseQueryService<FiscalYear, FiscalYearRequestModel, FiscalYearViewModel> fiscalYearQueryService,
            IBaseCommandService<FiscalYear> fiscalYearCommandService,
        IHttpContextAccessor contextAccessor)
        {
            _fiscalYearCommandService = fiscalYearCommandService;
            _fiscalYearQueryService = fiscalYearQueryService;
            _contextAccessor = contextAccessor;
        }

        [HttpPost]
      //  [Authorize(Policy = Permissions.FiscalYeares.ViewAll)]
       // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<FiscalYearViewModel>, int>>> Search(FiscalYearRequestModel request)
        {
            try
            {
                Tuple<List<FiscalYearViewModel>, int> content = await _fiscalYearQueryService.SearchAsync(request);
                return await Result<Tuple<List<FiscalYearViewModel>, int>>.SuccessAsync(content, "FiscalYeares Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<FiscalYearViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
       // [Authorize(Policy = Permissions.FiscalYeares.View)]
       // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<FiscalYearViewModel>> Detail(string id)
        {
            try
            {
                FiscalYearViewModel content = await _fiscalYearQueryService.GetDetailAsync(id);
                return await Result<FiscalYearViewModel>.SuccessAsync(content, "FiscalYear Found");
            }
            catch (Exception exception)
            {
                return await Result<FiscalYearViewModel>.FailAsync(exception.ToString(), exception.Message);
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
       // [Authorize(Policy = Permissions.FiscalYeares.Add)]
       // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] FiscalYear model)
        {
            try
            {
                model.Id = Guid.NewGuid().ToString();
                model.CreatedOn = DateTime.UtcNow;
                model.UpdatedOn= DateTime.UtcNow;
                model.CreatedBy = _contextAccessor.HttpContext.User.Identity.Name;
                model.UpdatedBy = _contextAccessor.HttpContext.User.Identity.Name;
                await _fiscalYearCommandService.AddAsync(model);
                return await Result<FiscalYear>.SuccessAsync(model , "FiscalYear Added Successfully");
            }
            catch (Exception exception)
            {
                return await Result<FiscalYear>.FailAsync(exception.ToString(), exception.Message);
            }
        }

     
     //   [Authorize(Policy = Permissions.FiscalYeares.Update)]
     //   [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/fiscalYear/update")]
        public virtual async Task<Result> PostUpdate([FromBody] FiscalYear model)
        {
            try
            { 
                model.UpdatedOn = DateTime.UtcNow;
                model.UpdatedBy = _contextAccessor.HttpContext.User.Identity.Name;
                await _fiscalYearCommandService.UpdateAsync(model);
                return await Result<FiscalYear>.SuccessAsync(model, "FiscalYear Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<FiscalYear>.FailAsync(exception.ToString(), exception.Message);
            }
          
        }
       // [Authorize(Policy = Permissions.FiscalYeares.Remove)]
       // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                FiscalYear fiscalYear = await _fiscalYearCommandService.FindAsync(id);
                if (fiscalYear != null)
                {
                    fiscalYear.Deleted = true;
                    fiscalYear.UpdatedOn = DateTime.UtcNow;
                    fiscalYear.UpdatedBy = _contextAccessor.HttpContext.User?.GetUserId();
                    await _fiscalYearCommandService.UpdateAsync(fiscalYear);
                }
                return await Result<string>.SuccessAsync(id, "FiscalYear Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}

