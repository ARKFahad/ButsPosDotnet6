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
    public class GenericNameController : ControllerBase
    {
        private readonly IBaseQueryService<GenericName, GenericNameRequestModel, GenericNameViewModel> _genericNameQueryService;
        private readonly IBaseCommandService<GenericName> _genericNameCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        public GenericNameController(IBaseQueryService<GenericName, GenericNameRequestModel, GenericNameViewModel> genericNameQueryService,
            IBaseCommandService<GenericName> genericNameCommandService,
            IHttpContextAccessor contextAccessor)
        {
            _genericNameCommandService = genericNameCommandService;
            _genericNameQueryService = genericNameQueryService;
            _contextAccessor = contextAccessor;
        }
        [Authorize(Policy = Permissions.GenericNames.ViewAll)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<GenericNameViewModel>, int>>> Search(GenericNameRequestModel request)
        {
            try
            {
                Tuple<List<GenericNameViewModel>, int> content = await _genericNameQueryService.SearchAsync(request);
                return await Result<Tuple<List<GenericNameViewModel>, int>>.SuccessAsync(content, "Generic Names Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<GenericNameViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.GenericNames.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<GenericNameViewModel>> Detail(string id)
        {
            try
            {
                GenericNameViewModel content = await _genericNameQueryService.GetDetailAsync(id);
                return await Result<GenericNameViewModel>.SuccessAsync(content, "GenericName Found");
            }
            catch (Exception exception)
            {
                return await Result<GenericNameViewModel>.FailAsync(exception.ToString(), exception.Message);
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
        [Authorize(Policy = Permissions.GenericNames.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] GenericName model)
        {
            try
            {
                model.Id = Guid.NewGuid().ToString();
                model.CreatedOn = DateTime.UtcNow;
                model.UpdatedOn = DateTime.UtcNow;
                model.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _genericNameCommandService.AddAsync(model);
                return await Result<GenericName>.SuccessAsync(model, "GenericName Added Successfully");
            }
            catch (Exception exception)
            {
                return await Result<GenericName>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.GenericNames.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/genericname/update")]
        public virtual async Task<Result> Put([FromBody] GenericName model)
        {
            try
            {
                model.UpdatedOn = DateTime.UtcNow;
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _genericNameCommandService.UpdateAsync(model);
                return await Result<GenericName>.SuccessAsync(model, "GenericName Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<GenericName>.FailAsync(exception.ToString(), exception.Message);
            }

        }
        [Authorize(Policy = Permissions.GenericNames.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                GenericName genericName = await _genericNameCommandService.FindAsync(id);
                if (genericName != null)
                {
                    genericName.Deleted = true;
                    genericName.UpdatedOn = DateTime.UtcNow;
                    genericName.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _genericNameCommandService.UpdateAsync(genericName);
                }
                return await Result<string>.SuccessAsync(id, "GenericName Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}
