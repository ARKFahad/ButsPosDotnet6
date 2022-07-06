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
    public class RoleController : ControllerBase
    {
        private readonly IBaseQueryService<Role, RoleRequestModel, RoleViewModel> _roleQueryService;
        private readonly IBaseCommandService<Role> _roleCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        public RoleController(IBaseQueryService<Role, RoleRequestModel, RoleViewModel> roleQueryService,
            IBaseCommandService<Role> roleCommandService,
            IHttpContextAccessor contextAccessor)
        {
            _roleCommandService = roleCommandService;
            _roleQueryService = roleQueryService;
            _contextAccessor = contextAccessor;
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Roles.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<RoleViewModel>, int>>> Search(RoleRequestModel request)
        {
            try
            {
                Tuple<List<RoleViewModel>, int> content = await _roleQueryService.SearchAsync(request);
                return await Result<Tuple<List<RoleViewModel>, int>>.SuccessAsync(content, "Roles Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<RoleViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Roles.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<RoleViewModel>> Detail(string id)
        {
            try
            {
                RoleViewModel content = await _roleQueryService.GetDetailAsync(id);
                return await Result<RoleViewModel>.SuccessAsync(content, "Role Found");
            }
            catch (Exception exception)
            {
                return await Result<RoleViewModel>.FailAsync(exception.ToString(), exception.Message);
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
        [Authorize(Policy = Permissions.Roles.Create)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] Role model)
        {
            try
            {
                model.Id = Guid.NewGuid().ToString();
                model.CreatedOn = DateTime.UtcNow;
                model.UpdatedOn = DateTime.UtcNow;
                model.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _roleCommandService.AddAsync(model);
                return await Result<Role>.SuccessAsync(model, "Role Added Successfully");
            }
            catch (Exception exception)
            {
                return await Result<Role>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Roles.Edit)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/role/update")]
        public virtual async Task<Result> Put([FromBody] Role model)
        {
            try
            {
                model.UpdatedOn = DateTime.UtcNow;
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _roleCommandService.UpdateAsync(model);
                return await Result<Role>.SuccessAsync(model, "Role Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<Role>.FailAsync(exception.ToString(), exception.Message);
            }

        }
        [Authorize(Policy = Permissions.Roles.Delete)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                Role role = await _roleCommandService.FindAsync(id);
                if (role != null)
                {
                    role.Deleted = true;
                    role.UpdatedOn = DateTime.UtcNow;
                    role.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _roleCommandService.UpdateAsync(role);
                }
               
                return await Result<string>.SuccessAsync(id, "Role Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}
