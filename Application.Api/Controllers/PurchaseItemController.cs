using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.RequestModel;
using Application.Services;
using Application.ViewModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PbApp.Service;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseItemController : ControllerBase
    {
        private readonly IBaseQueryService<PurchaseItem, PurchaseItemListRequestModel, PurchaseItemViewModel> _purchaseQueryService;
        private readonly IPdfService _pdfService;
        private readonly GHP_POS_DBContext _context;
        public PurchaseItemController(IBaseQueryService<PurchaseItem, PurchaseItemListRequestModel, PurchaseItemViewModel> purchaseQueryService,
            IPdfService pdfService,GHP_POS_DBContext context
           )
        {
            _purchaseQueryService = purchaseQueryService;
            _pdfService = pdfService;
            _context = context;
            
        }
        [Authorize(Policy = Permissions.PurchaseItems.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<PurchaseItemViewModel>, int>>> Search(PurchaseItemListRequestModel request)
        {
            try
            {
                Tuple<List<PurchaseItemViewModel>, int> content = await _purchaseQueryService.SearchAsync(request);

                return await Result<Tuple<List<PurchaseItemViewModel>, int>>.SuccessAsync(content, "Purchase Items Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<PurchaseItemViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.PurchaseItems.Print)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(PurchaseItemListRequestModel request)
        {
            try
            {
                request.Page = -1;
                request.IsAscending = true;
                Tuple<List<PurchaseItemViewModel>, int> content = await _purchaseQueryService.SearchAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {

                    _pdfService.PrintPurchaseItemReportToPdf(stream, _context.Branches.Find(request.BranchId), content.Item1.ToList(), "Purchase History", request.fromDate, request.ToDate);
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
