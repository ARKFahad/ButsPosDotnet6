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
    public class MeasurementUnitController : ControllerBase
    {
        private readonly IBaseQueryService<MeasurementUnit, MeasurementUnitRequestModel, MeasurementUnitViewModel> _measurementUnitQueryService;
        private readonly IBaseCommandService<MeasurementUnit> _measurementUnitCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        public MeasurementUnitController(IBaseQueryService<MeasurementUnit, MeasurementUnitRequestModel, MeasurementUnitViewModel> measurementUnitQueryService,
            IBaseCommandService<MeasurementUnit> measurementUnitCommandService,
            IHttpContextAccessor contextAccessor)
        {
            _measurementUnitCommandService = measurementUnitCommandService;
            _measurementUnitQueryService = measurementUnitQueryService;
            _contextAccessor = contextAccessor;
        }
        [Authorize(Policy = Permissions.MeasurementUnits.ViewAll)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<MeasurementUnitViewModel>, int>>> Search(MeasurementUnitRequestModel request)
        {
            try
            {
                Tuple<List<MeasurementUnitViewModel>, int> content = await _measurementUnitQueryService.SearchAsync(request);
                return await Result<Tuple<List<MeasurementUnitViewModel>, int>>.SuccessAsync(content, "Measurement Units Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<MeasurementUnitViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.MeasurementUnits.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<MeasurementUnitViewModel>> Detail(string id)
        {
            try
            {
                MeasurementUnitViewModel content = await _measurementUnitQueryService.GetDetailAsync(id);
                return await Result<MeasurementUnitViewModel>.SuccessAsync(content, "MeasurementUnit Found");
            }
            catch (Exception exception)
            {
                return await Result<MeasurementUnitViewModel>.FailAsync(exception.ToString(), exception.Message);
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
        [Authorize(Policy = Permissions.MeasurementUnits.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] MeasurementUnit model)
        {
            try
            {
                model.Id = Guid.NewGuid().ToString();
                model.CreatedOn = DateTime.UtcNow;
                model.UpdatedOn = DateTime.UtcNow;
                model.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _measurementUnitCommandService.AddAsync(model);
                return await Result<MeasurementUnit>.SuccessAsync(model, "MeasurementUnit Added Successfully");
            }
            catch (Exception exception)
            {
                return await Result<MeasurementUnit>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.MeasurementUnits.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/MeasurementUnit/update")]
        public virtual async Task<Result> Put([FromBody] MeasurementUnit model)
        {
            try
            {
                model.UpdatedOn = DateTime.UtcNow;
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _measurementUnitCommandService.UpdateAsync(model);
                return await Result<MeasurementUnit>.SuccessAsync(model, "MeasurementUnit Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<MeasurementUnit>.FailAsync(exception.ToString(), exception.Message);
            }

        }
        [Authorize(Policy = Permissions.MeasurementUnits.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                MeasurementUnit measurementUnit = await _measurementUnitCommandService.FindAsync(id);
                if (measurementUnit != null)
                {
                    measurementUnit.Deleted = true;
                    measurementUnit.UpdatedOn = DateTime.UtcNow;
                    measurementUnit.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _measurementUnitCommandService.UpdateAsync(measurementUnit);
                }
                return await Result<string>.SuccessAsync(id, "MeasurementUnit Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}
