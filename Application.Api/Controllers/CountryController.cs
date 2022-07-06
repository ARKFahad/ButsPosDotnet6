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
    public class CountryController : ControllerBase
    {
        private readonly IBaseQueryService<Country, CountryRequestModel, CountryViewModel> _countryQueryService;
        private readonly IBaseCommandService<Country> _countryCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        public CountryController(IBaseQueryService<Country, CountryRequestModel, CountryViewModel> countryQueryService,
            IBaseCommandService<Country> countryCommandService,
            IHttpContextAccessor contextAccessor)
        {
            _countryCommandService = countryCommandService;
            _countryQueryService = countryQueryService;
            _contextAccessor = contextAccessor;
        }
        [Authorize(Policy = Permissions.Countries.ViewAll)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<CountryViewModel>, int>>> Search(CountryRequestModel request)
        {
            try
            {
                Tuple<List<CountryViewModel>, int> content = await _countryQueryService.SearchAsync(request);
                return await Result<Tuple<List<CountryViewModel>, int>>.SuccessAsync(content, "Countries Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<CountryViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Countries.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<CountryViewModel>> Detail(string id)
        {
            try
            {
                CountryViewModel content = await _countryQueryService.GetDetailAsync(id);
                return await Result<CountryViewModel>.SuccessAsync(content, "Country Found");
            }
            catch (Exception exception)
            {
                return await Result<CountryViewModel>.FailAsync(exception.ToString(), exception.Message);
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
        [Authorize(Policy = Permissions.Countries.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] Country model)
        {
            try
            {
                model.Id = Guid.NewGuid().ToString();
                model.CreatedOn = DateTime.UtcNow;
                model.UpdatedOn = DateTime.UtcNow;
                model.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _countryCommandService.AddAsync(model);
                return await Result<Country>.SuccessAsync(model, "Country Added Successfully");
            }
            catch (Exception exception)
            {
                return await Result<Country>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Countries.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/country/update")]
        public virtual async Task<Result> Put([FromBody] Country model)
        {
            try
            {
                model.UpdatedOn = DateTime.UtcNow;
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _countryCommandService.UpdateAsync(model);
                return await Result<Country>.SuccessAsync(model, "Country Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<Country>.FailAsync(exception.ToString(), exception.Message);
            }

        }
        [Authorize(Policy = Permissions.Countries.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                Country country = await _countryCommandService.FindAsync(id);
                if (country != null)
                {
                    country.Deleted = true;
                    country.UpdatedOn = DateTime.UtcNow;
                    country.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _countryCommandService.UpdateAsync(country);
                }
                return await Result<string>.SuccessAsync(id, "Country Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}
