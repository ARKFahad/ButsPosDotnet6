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
    public class TransferDetailController : ControllerBase
    {
        private readonly IBaseQueryService<TransferDetail, TransferDetailListRequestModel, TransferDetailViewModel> _transferDetailQueryService;
        private readonly IPdfService _pdfService;
        private readonly GHP_POS_DBContext _context;
        public TransferDetailController(IBaseQueryService<TransferDetail, TransferDetailListRequestModel, TransferDetailViewModel> transferDetailQueryService,
            IPdfService pdfService,GHP_POS_DBContext context
           )
        {
            _transferDetailQueryService = transferDetailQueryService;
            _pdfService = pdfService;
            _context = context;
            
        }
        [Authorize(Policy = Permissions.Transfers.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<TransferDetailViewModel>, int>>> Search(TransferDetailListRequestModel request)
        {
            try
            {
                Tuple<List<TransferDetailViewModel>, int> content = await _transferDetailQueryService.SearchAsync(request);

                return await Result<Tuple<List<TransferDetailViewModel>, int>>.SuccessAsync(content, "Transfer Details Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<TransferDetailViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Transfers.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(TransferDetailListRequestModel request)
        {
            try
            {
                request.Page = -1;
                request.IsAscending = true;
                Tuple<List<TransferDetailViewModel>, int> content = await _transferDetailQueryService.SearchAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {

                   // _pdfService.PrintTransferDetailReportToPdf(stream, _context.Branches.Find(request.BranchId), content.Item1.ToList(), "Purchase History", request.fromDate, request.ToDate);
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
