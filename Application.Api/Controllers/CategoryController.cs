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
    public class CategoryController : ControllerBase
    {
        private readonly IBaseQueryService<Category, CategoryRequestModel, CategoryViewModel> _categoryQueryService;
        private readonly IBaseCommandService<Category> _categoryCommnadService;
        private readonly IHttpContextAccessor _contextAccessor;
        public CategoryController(IBaseQueryService<Category, CategoryRequestModel, CategoryViewModel> categoryQueryService,
            IBaseCommandService<Category> categoryCommnadService,
            IHttpContextAccessor contextAccessor)
        {
            _categoryCommnadService = categoryCommnadService;
            _categoryQueryService = categoryQueryService;
            _contextAccessor = contextAccessor;
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Categories.ViewAll)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<CategoryViewModel>, int>>> Search(CategoryRequestModel request)
        {
            try
            {
                Tuple<List<CategoryViewModel>, int> content = await _categoryQueryService.SearchAsync(request);
                return await Result<Tuple<List<CategoryViewModel>, int>>.SuccessAsync(content, "Categories Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<CategoryViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Categories.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<CategoryViewModel>> Detail(string id)
        {
            try
            {
                CategoryViewModel content = await _categoryQueryService.GetDetailAsync(id);
                return await Result<CategoryViewModel>.SuccessAsync(content, "Category Found");
            }
            catch (Exception exception)
            {
                return await Result<CategoryViewModel>.FailAsync(exception.ToString(), exception.Message);
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
        [Authorize(Policy = Permissions.Categories.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] Category model)
        {
            try
            {
                model.Id = Guid.NewGuid().ToString();
                model.CreatedOn = DateTime.Now;
                model.UpdatedOn= DateTime.Now;
                model.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _categoryCommnadService.AddAsync(model);
                return await Result<Category>.SuccessAsync(model , "Category Added Successfully");
            }
            catch (Exception exception)
            {
                return await Result<Category>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Categories.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/category/update")]
        public virtual async Task<Result> Put([FromBody] Category model)
        {
            try
            { 
                model.UpdatedOn = DateTime.Now;
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _categoryCommnadService.UpdateAsync(model);
                return await Result<Category>.SuccessAsync(model, "Category Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<Category>.FailAsync(exception.ToString(), exception.Message);
            }
          
        }
        [Authorize(Policy = Permissions.Categories.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                Category category = await _categoryCommnadService.FindAsync(id);
                if (category != null)
                {
                    category.Deleted = true;
                    category.UpdatedOn = DateTime.Now;
                    category.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _categoryCommnadService.UpdateAsync(category);
                }
                return await Result<string>.SuccessAsync(id, "Category Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}

