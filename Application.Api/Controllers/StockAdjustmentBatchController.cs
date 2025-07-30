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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PbApp.Service;
using static Application.Core.Constants.Permissions;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockAdjustmentBatchController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IPdfService _pdfService;
        private readonly IBaseQueryService<StockAdjustmentBatch, StockAdjustmentBatchRequestModel, StockAdjustmentBatchViewModel> _stockAdjustmentBatchQueryService;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;

        public StockAdjustmentBatchController(GHP_POS_DBContext context, IHttpContextAccessor contextAccessor, IPdfService pdfService, IBaseQueryService<StockAdjustmentBatch, StockAdjustmentBatchRequestModel, StockAdjustmentBatchViewModel> stockAdjustmentBatchQueryService, ICommonService commonService, IConfiguration configuration)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _pdfService = pdfService;
            _stockAdjustmentBatchQueryService = stockAdjustmentBatchQueryService;
            _commonService = commonService;
            _configuration = configuration;
        }

        [Authorize(Policy = Permissions.PurchaseReturns.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<StockAdjustmentBatchViewModel>, int>>> Search(StockAdjustmentBatchRequestModel request)
        {
            try
            {
                Tuple<List<StockAdjustmentBatchViewModel>, int> content = await _stockAdjustmentBatchQueryService.SearchAsync(request);
                return await Result<Tuple<List<StockAdjustmentBatchViewModel>, int>>.SuccessAsync(content, "Stock Adjustment Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<StockAdjustmentBatchViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        [Authorize(Policy = Permissions.PurchaseReturns.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> AddStockAdjustment([FromBody] StockAdjustmentBatchModel model)
        {
            var currentUserId = _contextAccessor.HttpContext?.User?.GetUserId();
            var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var count = _context.StockAdjustmentBatches.Count() + 1;
                    var code = "SAB" + count.ToString().PadLeft(7, '0');
                    StockAdjustmentBatch stockAdjustmentBatch = new StockAdjustmentBatch
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = code,
                        BranchId = model.BranchId,
                        Remark = model.Remark,
                        CreatedOn = DateTime.Now,
                        UpdatedOn = DateTime.Now,
                        CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId() ?? "me",
                        UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId() ?? "me"
                    };

                    var stockAdjustmentValue = 0M;
                    foreach (var item in model.StockAdjustmentBatchItems)
                    {
                        if (item.Quantity > 0)
                        {
                            for (int i = 0; i < item.Quantity; i++)
                            {
                                await _context.StockItems.AddAsync(new StockItem
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    ProductId = item.ProductId,
                                    SupplierId = item.SupplierId,
                                    Quantity = 1,
                                    PurchaseRate = item.PurchaseRate,
                                    ExpiryDate = item.ExpiryDate,
                                    CreatedOn = DateTime.Now,
                                    CreatedBy = currentUserId ?? "me"
                                });
                            }
                            await _context.StockAdjustmentBatchItems.AddAsync(new StockAdjustmentBatchItem
                            {
                                Id = Guid.NewGuid().ToString(),
                                StockAdjustmentBatchId = stockAdjustmentBatch.Id,
                                ProductId = item.ProductId,
                                SupplierId = item.SupplierId,
                                Quantity = item.Quantity,
                                PurchaseRate = item.PurchaseRate,
                                CreatedOn = DateTime.Now,
                                CreatedBy = currentUserId ?? "me"
                            });
                            var stockValue = item.PurchaseRate * item.Quantity;
                            stockAdjustmentValue += stockValue;
                        }
                        else
                        {
                            if (item.Quantity == 0)
                            {
                                var product = _context.Products.Where(x => x.Id == item.ProductId).First();
                                throw new BadRequestException($"0 can not be required quantity. Please add valid quantity for {product.Name}");
                            }

                            var absoluteQuantity = Math.Abs(item.Quantity);
                            var stockItems = await _context.StockItems.Where(x => x.ProductId == item.ProductId && x.BranchId == model.BranchId).OrderBy(x => x.CreatedOn).Take(absoluteQuantity).ToListAsync();
                            if (!stockItems.Any())
                                throw new NotFoundResultException("Required Stock Quantity not available In Selected Branch");
                            decimal stockValue = 0M;
                            foreach (var stockItem in stockItems)
                            {
                                stockValue += ((stockItem.PurchaseRate - stockItem.DiscountPerItem) + stockItem.AdditionalCostPerItem);
                            }
                            stockAdjustmentValue -= stockValue;
                            _context.StockItems.RemoveRange(stockItems);
                            await _context.StockAdjustmentBatchItems.AddAsync(new StockAdjustmentBatchItem
                            {
                                Id = Guid.NewGuid().ToString(),
                                StockAdjustmentBatchId = stockAdjustmentBatch.Id,
                                ProductId = item.ProductId,
                                SupplierId = item.SupplierId,
                                Quantity = item.Quantity,
                                PurchaseRate = stockValue / absoluteQuantity,
                                CreatedOn = DateTime.Now,
                                CreatedBy = currentUserId ?? "me"
                            });

                        }
                    }

                    if (stockAdjustmentValue > 0)
                    {
                        if (isAccountsActive)
                        {
                            //Owners Capital Credit
                            await _commonService.HitAccount(_context, model.BranchId, code, "Stock Adjustment",
                               AccountHeadConstants.MASUDULHASAN, 0, stockAdjustmentValue, DateTime.Now);
                            //inventory a/c debit
                            await _commonService.HitAccount(_context, model.BranchId, code, "Stock Adjustment",
                               AccountHeadConstants.InventoryAC, stockAdjustmentValue, 0, DateTime.Now);
                        }
                    }
                    else
                    {
                        if (isAccountsActive)
                        {
                            //Owners Capital Debit
                            await _commonService.HitAccount(_context, model.BranchId, code, "Stock Adjustment",
                               AccountHeadConstants.MASUDULHASAN, Math.Abs(stockAdjustmentValue), 0, DateTime.Now);
                            //inventory a/c credit
                            await _commonService.HitAccount(_context, model.BranchId, code, "Stock Adjustment",
                               AccountHeadConstants.InventoryAC, 0, Math.Abs(stockAdjustmentValue), DateTime.Now);
                        }
                    }

                    await _context.StockAdjustmentBatches.AddAsync(stockAdjustmentBatch);
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return Ok(new { Message = "Stock Adjustment Added Successfully", Succeeded = true });
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return Ok(new { StockAdjustmentId = "", Message = exception.Message, Succeeded = false });
                }

            }
        }

        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(StockAdjustmentBatchRequestModel request)
        {
            try
            {
                request.Page = -1;
                request.IsAscending = true;
                Tuple<List<StockAdjustmentBatchViewModel>, int> content = await _stockAdjustmentBatchQueryService.SearchAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _pdfService.PrintStockAdjustmentBatchReportToPdf(stream, content.Item1, "Stock Adjustment Report ", _context.Branches.Find(request.BranchId), request.fromDate, request.ToDate);
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
