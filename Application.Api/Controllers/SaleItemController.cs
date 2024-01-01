using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.RequestModel;
using Application.Services;
using Application.ViewModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PbApp.Service;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleItemController : ControllerBase
    {
        private readonly IBaseQueryService<SaleItem, SaleItemListRequestModel, SaleItemViewModel> _SaleQueryService;
        private readonly IPdfService _pdfService;
        private readonly GHP_POS_DBContext _context;

        public SaleItemController(IBaseQueryService<SaleItem, SaleItemListRequestModel, SaleItemViewModel> SaleQueryService,
            IPdfService pdfService, GHP_POS_DBContext context
           )
        {
            _SaleQueryService = SaleQueryService;
            _pdfService = pdfService;
            _context = context;

        }

        [Authorize(Policy = Permissions.SaleItems.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<SaleItemViewModel>, int>>> Search(SaleItemListRequestModel request)
        {
            try
            {
                Tuple<List<SaleItemViewModel>, int> content = await _SaleQueryService.SearchAsync(request);
                return await Result<Tuple<List<SaleItemViewModel>, int>>.SuccessAsync(content, "Sale Items Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<SaleItemViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        [Authorize(Policy = Permissions.SaleItems.Print)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(SaleItemListRequestModel request)
        {
            try
            {
                request.Page = -1;
                request.IsAscending = true;
                Tuple<List<SaleItemViewModel>, int> content = await _SaleQueryService.SearchAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintSaleItemReportToPdf(stream, _context.Branches.Find(request.BranchId), content.Item1.ToList(), "Sale History", request.fromDate, request.ToDate);
                    bytes = stream.ToArray();
                    bytes = _pdfService.AddPageNumbers(bytes);
                }

                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }
    }
}
