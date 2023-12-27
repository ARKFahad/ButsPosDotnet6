using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.PermissionHelpers;
using Application.RequestModel;
using Application.Services;
using Application.ViewModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PbApp.Service;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockAdjustmentController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IPdfService _pdfService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ICommonService _commonService;
        private readonly IBaseQueryService<StockAdjustment, StockAdjustmentRequestModel, StockAdjustmentViewModel> _stockAdjustmentQueryService;
        public StockAdjustmentController(GHP_POS_DBContext context, IPdfService pdfService, IHttpContextAccessor contextAccessor,
            IConfiguration configuration,
            ICommonService commonService,
            IBaseQueryService<StockAdjustment, StockAdjustmentRequestModel, StockAdjustmentViewModel> stockAdjustmentQueryService)
        {
            _context = context;
            _pdfService = pdfService;
            _contextAccessor = contextAccessor;
            _configuration = configuration;
            _commonService = commonService;
            _stockAdjustmentQueryService = stockAdjustmentQueryService;
        }
        [Authorize(Policy = Permissions.StockAdjustments.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<StockAdjustmentViewModel>, int>>> Search(StockAdjustmentRequestModel request)
        {
            try
            {
                Tuple<List<StockAdjustmentViewModel>, int> content = await _stockAdjustmentQueryService.SearchAsync(request);
                return await Result<Tuple<List<StockAdjustmentViewModel>, int>>.SuccessAsync(content, "Stock Adjustment Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<StockAdjustmentViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }


         [Authorize(Policy = Permissions.StockAdjustments.Add)]
         [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> AddAdjustment([FromBody] StockAdjustmentModel model)
        {
            var currentUserId = _contextAccessor.HttpContext?.User?.GetUserId();
            var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var count = _context.StockAdjustments.Count() + 1;
                    var code = "SA" + count.ToString().PadLeft(7, '0');
                    if (model.Quantity > 0)
                    {
                        for (int i = 0; i < model.Quantity; i++)
                        {
                            await _context.StockItems.AddAsync(new StockItem
                            {
                                Id = Guid.NewGuid().ToString(),
                                ProductId = model.ProductId,
                                SupplierId = model.SupplierId,
                                BranchId = model.BranchId,
                                Quantity = 1,
                                PurchaseRate = model.PurchaseRate,
                                ExpiryDate = DateTime.Now.AddYears(2),
                                CreatedOn = DateTime.Now,
                                CreatedBy = currentUserId ?? "me"
                            });
                        }
                        var stockValue = model.PurchaseRate * model.Quantity;
                        if (isAccountsActive)
                        {
                            //Owners Capital Credit
                            await _commonService.HitAccount(_context, model.BranchId, code, "Stock Adjustment",
                               AccountHeadConstants.MASUDULHASAN, 0, stockValue, DateTime.Now);
                            //inventory a/c debit
                            await _commonService.HitAccount(_context, model.BranchId, code, "Stock Adjustment",
                               AccountHeadConstants.InventoryAC, stockValue, 0, DateTime.Now);
                        }
                        _context.StockAdjustments.Add(new StockAdjustment
                        {
                            Id = Guid.NewGuid().ToString(),
                            Code = code,
                            ProductId = model.ProductId,
                            SupplierId = model.SupplierId,
                            BranchId = model.BranchId,
                            Quantity = model.Quantity,
                            PurchaseRate = model.PurchaseRate,
                            CreatedOn = DateTime.Now,
                            CreatedBy = currentUserId ?? "me"

                        });
                    }
                    else
                    {
                        if (model.Quantity == 0) return await Result<string>.FailAsync("", "Quantity can not be 0");
                        var absoluteQuantity= Math.Abs(model.Quantity);
                        var stockItems = await _context.StockItems.Where(x => x.ProductId == model.ProductId && x.BranchId == model.BranchId).OrderBy(x => x.CreatedOn).Take(absoluteQuantity).ToListAsync();
                        if (!stockItems.Any())
                            throw new NotFoundResultException("Required Stock Quantity not available In Selected Branch");
                        decimal stockValue = 0M;
                        foreach (var stockItem in stockItems)
                        {
                            stockValue += ((stockItem.PurchaseRate - stockItem.DiscountPerItem) + stockItem.AdditionalCostPerItem);
                        }
                        _context.StockItems.RemoveRange(stockItems);
                        if (isAccountsActive)
                        {
                            //Owners Capital Debit
                            await _commonService.HitAccount(_context, model.BranchId, code, "Stock Adjustment",
                               AccountHeadConstants.MASUDULHASAN, stockValue,0, DateTime.Now);
                            //inventory a/c credit
                            await _commonService.HitAccount(_context, model.BranchId, code, "Stock Adjustment",
                               AccountHeadConstants.InventoryAC, 0,stockValue, DateTime.Now);
                        }
                        _context.StockAdjustments.Add(new StockAdjustment
                        {
                            Id = Guid.NewGuid().ToString(),
                            Code = code,
                            ProductId = model.ProductId,
                            SupplierId = model.SupplierId,
                            BranchId = model.BranchId,
                            Quantity = model.Quantity,
                            PurchaseRate = stockValue/ absoluteQuantity,
                            CreatedOn = DateTime.Now,
                            CreatedBy = currentUserId ?? "me"
                        });

                    }
                                     
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return await Result<string>.SuccessAsync("", "Stock Adjusted Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return await Result<string>.FailAsync(exception.ToString(), exception.Message);
                }

            }
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(StockAdjustmentRequestModel request)
        {
            try
            {
                request.Page = -1;
                request.IsAscending = true;
                Tuple<List<StockAdjustmentViewModel>, int> content = await _stockAdjustmentQueryService.SearchAsync(request);
                byte[] bytes;

                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintStockAdjustmentReportToPdf(stream, content.Item1, "Stock Adjustment Report ", _context.Branches.Find(request.BranchId), request.fromDate, request.ToDate);
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
