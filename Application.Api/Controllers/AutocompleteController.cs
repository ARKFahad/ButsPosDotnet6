using Application.Core.Common;
using Application.Services;
using Application.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutocompleteController : ControllerBase
    {
        private readonly ICommonService _commonService;
        public AutocompleteController(ICommonService commonService)
        {
            _commonService = commonService;
        }
        [HttpGet]
        [Route("{supplierid}")]
        public virtual async Task<Result<List<ProductAutocompleteViewModel>>> SearchProductAutocomplete(string supplierid)
        {
            try
            {
                var list=await _commonService.SearchProductAutocomplete(supplierid:supplierid);
                return await Result<List<ProductAutocompleteViewModel>>.SuccessAsync(list, "Products Found");
            }
            catch (Exception exception)
            {
                return await Result < List < ProductAutocompleteViewModel >>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}
