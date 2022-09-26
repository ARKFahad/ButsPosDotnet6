using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.PermissionHelpers;
using Application.Core.SignalR;
using Application.RequestModel;
using Application.Services;
using Application.ViewModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PbApp.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IPdfService _pdfService;
        private readonly IBaseQueryService<Sale, SaleRequestModel, SaleViewModel> _saleQueryService;
        private readonly IConfiguration _configuration;
        private readonly ICommonService _commonService;
        private readonly IAccountService _accountService;
        private readonly IHubContext<BroadcastHub,IHubClient> _hubContext;
        public SaleController(IBaseQueryService<Sale, SaleRequestModel, SaleViewModel> saleQueryService, 
            GHP_POS_DBContext context, IHttpContextAccessor contextAccessor, IPdfService pdfService,
            IConfiguration configuration,
            ICommonService commonService,
            IAccountService accountService,
            IHubContext<BroadcastHub, IHubClient> hubContext)
        {
            _saleQueryService = saleQueryService;
            _context = context;
            _contextAccessor = contextAccessor;
            _pdfService = pdfService;
            _configuration = configuration;
            _commonService = commonService;
            _accountService = accountService;
            _hubContext = hubContext;
        }
        [Authorize(Policy = Permissions.Sales.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]

        public virtual async Task<Result<Tuple<List<SaleViewModel>, int>>> Search(SaleRequestModel request)
        {
            try
            {
                Tuple<List<SaleViewModel>, int> content = await _saleQueryService.SearchAsync(request);
                return await Result<Tuple<List<SaleViewModel>, int>>.SuccessAsync(content, "Sales Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<SaleViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(SaleRequestModel request) 
        {
            try
            {
                request.Page = -1;
                request.IsAscending = true;
                Tuple<List<SaleViewModel>, int> content = await _saleQueryService.SearchAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintSaleReportToPdf(stream, _context.Branches.Find(request.BranchId), content.Item1.ToList(), "Sale Report ", request.fromDate,request.ToDate);
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

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Policy = Permissions.Sales.Add)]
        public async Task<IActionResult> AddSaleAsync([FromBody] SaleAddRequestModel model)
           
        {
            var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var branchId = _contextAccessor.HttpContext?.User?.GetBranchId();
                    var dbCustomer = await _context.Customers.FindAsync(model.CustomerId);
                    Sale sale = new Sale();
                    var count = _context.Sales.Count() + 1;
                        sale.Id = Guid.NewGuid().ToString();
                        sale.SaleNo = "S" + count.ToString().PadLeft(7, '0');
                        sale.CustomerId = model.CustomerId;
                        sale.BranchId = branchId;
                        sale.Subtotal = model.Total;
                        sale.Discount = model.Discount;
                        sale.DiscountInPercentage=model.DiscountInPercentage;
                        sale.PayableAmount = model.NetTotal;
                        sale.RecievedAmount = model.ReceivedAmount;
                        sale.ReturnAmount = model.ReturnAmount;
                        sale.PaidAmount = model.PaymentAmount;
                        sale.DueAmount = model.DueAmount;
                        sale.CreatedOn = DateTime.Now;
                        sale.UpdatedOn = DateTime.Now;
                        sale.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                        sale.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    foreach (var item in model.CartItems)
                        {
                        var saleItem = new SaleItem
                        {
                            Id = Guid.NewGuid().ToString(),
                            SaleId = sale.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            ReturnQuantity = 0,
                            Amount = item.Quantity * item.Price,
                            CreatedOn = DateTime.Now,
                            BranchId = branchId,
                            DiscountOnAmount = (sale.DiscountInPercentage * (item.Quantity * item.Price)) / 100
                        };
                            var stockItems = await _context.StockItems.Where(x => x.ProductId == saleItem.ProductId && x.BranchId == branchId).OrderBy(x => x.CreatedOn).Take(saleItem.Quantity).ToListAsync();
                            decimal cogsItem = 0M;
                            foreach (var stockItem in stockItems)
                            {
                               var cogsperItem= ((stockItem.PurchaseRate - stockItem.DiscountPerItem) + stockItem.AdditionalCostPerItem);
                                cogsItem += cogsperItem;
                                sale.Cogs += cogsperItem;
                            }
                            //cogs perItem
                            saleItem.CogsPerItem = cogsItem / saleItem.Quantity;
                            saleItem.SupplierId= stockItems[0].SupplierId;
                            sale.SaleItems.Add(saleItem);
                            //stock check
                            var stockCount = _context.StockItems.Where(x => x.ProductId == item.ProductId && x.BranchId == branchId).Sum(x => x.Quantity);
                            if (item.Quantity > stockCount) throw new Exception("Out of Stock");
                            _context.StockItems.RemoveRange(stockItems);
                             await _context.SaveChangesAsync();
                    }                      
                    if (isAccountsActive)
                    {
                        if (sale.DueAmount > 0)
                        {
                            sale.CustomerBalance =  _accountService.CalculateBalanceByAccountId(dbCustomer.AccountId, AccType.Asset,_context.FiscalYears.FirstOrDefault(x=>x.IsActive)?.Id,null,null, branchId) + model.DueAmount;
                            //sale a/c credit
                            await _commonService.HitAccount(_context, sale.BranchId, sale.SaleNo, "Credit Sale",
                                AccountHeadConstants.Sales, 0, sale.DueAmount,sale.CreatedOn);
                            //customer a/c debit
                            await _commonService.HitAccount(_context, sale.BranchId, sale.SaleNo, "Credit Sale",
                               dbCustomer.AccountId, sale.DueAmount, 0, sale.CreatedOn);
                            //inventory a/c credit
                            await _commonService.HitAccount(_context, sale.BranchId, sale.SaleNo, "Credit Sale",
                                AccountHeadConstants.InventoryAC, 0, sale.Cogs, sale.CreatedOn);
                            //cogs a/c debit
                            await _commonService.HitAccount(_context, sale.BranchId, sale.SaleNo, "Credit Sale",
                               AccountHeadConstants.COGS, sale.Cogs, 0, sale.CreatedOn);
                        }
                        else
                        {
                            //sale a/c credit
                            await _commonService.HitAccount(_context, sale.BranchId, sale.SaleNo, "Cash Sale",
                                AccountHeadConstants.Sales, 0, sale.PaidAmount, sale.CreatedOn);
                            //cash a/c debit
                            await _commonService.HitAccount(_context, sale.BranchId, sale.SaleNo, "Cash Sale",
                              AccountHeadConstants.CashInHand, sale.PaidAmount, 0, sale.CreatedOn);
                            //inventory a/c credit
                            await _commonService.HitAccount(_context, sale.BranchId, sale.SaleNo, "Cash Sale",
                                AccountHeadConstants.InventoryAC, 0, sale.Cogs, sale.CreatedOn);
                            //cogs a/c debit
                            await _commonService.HitAccount(_context, sale.BranchId, sale.SaleNo, "Cash Sale",
                               AccountHeadConstants.COGS, sale.Cogs, 0, sale.CreatedOn);
                        }
                    }
                    else
                    {
                        //Customer Balance Update
                        if (model.DueAmount > 0)
                        {
                            sale.CustomerBalance = dbCustomer.Balance + model.DueAmount;
                            dbCustomer.Balance += sale.DueAmount;
                            _context.Customers.Update(dbCustomer);
                        }
                    }
                    await _context.Sales.AddAsync(sale);
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    //integrate signalR
                    await _hubContext.Clients.All.BroadcastMessage();
                    return Ok(new { SaleId = sale.Id,Message="Sale Added Successfully" });
                }
                catch (Exception e)
                {
                    await dbContextTransaction.RollbackAsync();
                    return Ok(new { SaleId = "", Message = e.Message });
                }
            }
        }
        [HttpGet("/api/sale/invoice/{id}")]
        public IActionResult SaleInvoice(string id= "de955566-4795-4048-9c61-9893a3dc6f8f")
        {
            var sale = _context.Sales.Find(id);
            if (sale == null) throw new NotFoundResultException("Not Found");
            var customer = _context.Customers.Find(sale.CustomerId);
            var branch = _context.Branches.Find(sale.BranchId);
            var saleItems = _context.SaleItems.Include(x=>x.Product).Where(x => x.SaleId == sale.Id).Select(x => new SaleItemModel
            {
                Description=x.Product.Name+" "+x.Product.PackSize,
                Quantity=x.Quantity,
                Price=x.Price,
                Amount=x.Amount
            }).ToList();
            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintSaleInvoiceToPdf(stream, customer,branch,sale,saleItems);
                    bytes = stream.ToArray();
                   // bytes =_pdfService.AddFooterInfoForA5Page(bytes);
                }
                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exc)
            {
                throw;
            }

        }

        [Authorize(Policy = Permissions.Sales.ItemWiseSaleProfitLoss)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("item-wise-sale-profit-loss")]
        public virtual async Task<Result<Tuple<List<ItemSaleProfitLossViewModel>, int>>> ItemWiseProfitLoss(ItemSaleProfitLossRequestModel request)
        {
            try
            {
               var plQueryable=ItemWiseProfitLossQueryable(request);
                var count = plQueryable.Count();
                if (request.Page != -1)
                    plQueryable = plQueryable.Skip(request.Page * request.RowsPerPage).Take(request.RowsPerPage);
                var list = await plQueryable.ToListAsync();
                Tuple<List<ItemSaleProfitLossViewModel>, int> content = new Tuple<List<ItemSaleProfitLossViewModel>, int>(list, count);
                return await Result<Tuple<List<ItemSaleProfitLossViewModel>, int>>.SuccessAsync(content, "Item Wise Sale Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<ItemSaleProfitLossViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpPost]
        [Route("item-wise-sale-profit-loss-print")]
        public virtual async Task<IActionResult> ItemWiseProfitLossPrint(ItemSaleProfitLossRequestModel request)
        {
            try
            {
                var plList = await ItemWiseProfitLossQueryable(request).ToListAsync();
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintItemWiseSaleProfitLossReportToPdf(stream, _context.Branches.Find(request.BranchId), plList, "Item Wise Sale Profit Loss Report ", request.FromDate, request.ToDate);
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


        private IQueryable<ItemSaleProfitLossViewModel> ItemWiseProfitLossQueryable(ItemSaleProfitLossRequestModel request)
        {
            
                var query = _context.SaleItems.AsQueryable();
                if (!string.IsNullOrWhiteSpace(request.BranchId))
                    query = query.Where(x => x.BranchId == request.BranchId);
                //hardcoded initial value
                if (!request.FromDate.HasValue)
                    request.FromDate = DateTime.Now.AddYears(-5);
                if (!request.ToDate.HasValue)
                    request.ToDate = DateTime.Now;
                query = query.Where(x => x.CreatedOn.Date >= request.FromDate.Value.Date && x.CreatedOn.Date <= request.ToDate.Value.Date);
                var queryable = query.GroupBy(x => new { x.ProductId, x.SupplierId })
                          .Select(y => new
                          {
                              ProductId = y.Key.ProductId,
                              SupplierId = y.Key.SupplierId,
                              SaleQuantity = y.Sum(x => x.Quantity),
                              ReturnQuantity = y.Sum(x => x.ReturnQuantity),
                              Subtotal = y.Sum(x => x.Amount),
                              Discount = y.Sum(x => x.DiscountOnAmount),
                              NetAmount = y.Sum(x => x.Amount) - y.Sum(x => x.DiscountOnAmount),
                              COGS = y.Sum(x => x.CogsPerItem * x.Quantity)
                          });
                if (!string.IsNullOrWhiteSpace(request.SupplierId))
                    queryable = queryable.Where(x => x.SupplierId == request.SupplierId);
                if (!string.IsNullOrWhiteSpace(request.ProductId))
                    queryable = queryable.Where(x => x.ProductId == request.ProductId);

                var plQueryable = from saleItem in queryable
                                  join product in _context.Products on saleItem.ProductId equals product.Id
                                  join category in _context.Categories on product.CategoryId equals category.Id
                                  join supplier in _context.Suppliers on saleItem.SupplierId equals supplier.Id
                                  select new ItemSaleProfitLossViewModel()
                                  {
                                      ProductName = product.Name + " " + product.PackSize + " " + product.Category.Name,
                                      SupplierName = supplier.Name,
                                      SaleQuantity = saleItem.SaleQuantity,
                                      ReturnQuantity = saleItem.ReturnQuantity,
                                      Subtotal = saleItem.Subtotal,
                                      Discount = saleItem.Discount,
                                      NetAmount = saleItem.NetAmount,
                                      Cogs = saleItem.COGS,
                                  };
                 return plQueryable;
         }
    }
}



