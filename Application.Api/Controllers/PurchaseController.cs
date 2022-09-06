using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Enums;
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
using Microsoft.Extensions.Configuration;
using PbApp.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        private readonly IBaseQueryService<Purchase, PurchaseRequestModel, PurchaseViewModel> _purchaseQueryService;
        private readonly IBaseCommandService<Purchase> _purchaseCommandService;
        private readonly IBaseCommandService<Supplier> _supplierCommnadService;
        private readonly IBaseCommandService<StockItem> _stockItemCommnadService;
        private readonly IBaseCommandService<PurchaseItem> _purchaseItemCommnadService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly GHP_POS_DBContext _context;
        private readonly IPdfService _pdfService;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;
        private readonly IAccountService _accountService;
        public PurchaseController(IBaseQueryService<Purchase, PurchaseRequestModel, PurchaseViewModel> purchaseQueryService,
           IBaseCommandService<Purchase> purchaseCommandService,
           IBaseCommandService<Supplier> supplierCommnadService,
           IBaseCommandService<StockItem> stockItemCommnadService,
           IBaseCommandService<PurchaseItem> purchaseItemCommnadService,
           IHttpContextAccessor contextAccessor, GHP_POS_DBContext context, 
           IPdfService pdfService,
           ICommonService commonService,
           IConfiguration configuration,
           IAccountService accountService)
        {
            _purchaseQueryService = purchaseQueryService;
            _purchaseCommandService = purchaseCommandService;
            _stockItemCommnadService = stockItemCommnadService;
            _supplierCommnadService = supplierCommnadService;
            _purchaseItemCommnadService = purchaseItemCommnadService;
            _contextAccessor = contextAccessor;
            _context = context;
            _pdfService = pdfService;
            _commonService = commonService;
            _configuration = configuration;
            _accountService = accountService;
        }
        [Authorize(Policy = Permissions.Purchases.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<PurchaseViewModel>, int>>> Search(PurchaseRequestModel request)
        {
            try
            {
                Tuple<List<PurchaseViewModel>, int> content = await _purchaseQueryService.SearchAsync(request);
                
                return await Result<Tuple<List<PurchaseViewModel>, int>>.SuccessAsync(content, "Purchases Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<PurchaseViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(PurchaseRequestModel request)
        {
            try
            {
                request.Page = -1;
                request.IsAscending = true;
                Tuple<List<PurchaseViewModel>, int> content = await _purchaseQueryService.SearchAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                   
                    _pdfService.PrintPurchaseReportToPdf(stream, _context.Branches.Find(request.BranchId), content.Item1.ToList(), "Purchase Report ",request.fromDate,request.ToDate);
                    bytes = stream.ToArray();
                    bytes = _pdfService.AddPageNumbers(bytes);
                }

                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return  Ok(exception.ToString());
            }
        }
        [Authorize(Policy = Permissions.Purchases.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<PurchaseViewModel>> Detail(string id)
        {
            try
            {
                PurchaseViewModel content = await _purchaseQueryService.GetDetailAsync(x => x.Id == id, "Branch;PurchaseItems;Supplier");
                return await Result<PurchaseViewModel>.SuccessAsync(content, "Product Found");
            }
            catch (Exception exception)
            {
                return await Result<PurchaseViewModel>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Purchases.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("purchase-no")]
        [HttpGet]
        public IActionResult GetPurcahseNo()
        {
            string purchaseNo = _purchaseCommandService.GetReferanceId(character: "P");
            return Ok(new { PurchaseNo = purchaseNo });
        }

        [Authorize(Policy = Permissions.Purchases.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("purchase-item-stock-count")]
        [HttpGet]
        public async Task<Result> AvailableStockQuantityByPurchaseItemId(string id)
        {
            var stockItems = await _stockItemCommnadService.GetAllByPredicate(x => x.PurchaseItemId == id);
            return await Result<int>.SuccessAsync(stockItems.Count(), "Stock Available");
        }

        [Authorize(Policy = Permissions.Purchases.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> AddPurchase([FromBody] PurchaseAddRequestModel model)
        {
            var currentUserId= _contextAccessor.HttpContext?.User?.GetUserId(); 
            var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
            var isValidDate = DateTime.TryParseExact(model.PurchaseDate, "dd/MM/yyyy", null, DateTimeStyles.None, out DateTime purchaseDate);
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var dbSupplier = await _context.Suppliers.FindAsync(model.SupplierId);
                    Purchase purchase = new Purchase();
                    var subtotal = CalculateSubtotalAndTotalVat(model.PurchaseItems,out decimal totalVat);
                    purchase.Id = Guid.NewGuid().ToString();
                    purchase.PurchaseNo = _purchaseCommandService.GetReferanceId("P");
                    purchase.PurchaseDate = isValidDate ? purchaseDate : DateTime.Now;
                    purchase.SupplierId = model.SupplierId;
                    purchase.ReferenceNo = model.ReferenceNo;
                    purchase.BranchId = model.BranchId;
                    purchase.TotalValue = subtotal - totalVat;
                    purchase.TotalVat = totalVat;
                    purchase.Subtotal = subtotal;
                    purchase.Discount = model.Discount;
                    purchase.AdditionalCost = model.AdditionalCost;
                    purchase.NetAmount = subtotal + model.AdditionalCost - model.Discount;
                    purchase.PurchaseType = model.PurchaseType;
                    purchase.Remark = model.Remark;
                    purchase.CreatedOn = DateTime.Now;
                    purchase.UpdatedOn = DateTime.Now;
                    purchase.CreatedBy = currentUserId;
                    purchase.UpdatedBy = currentUserId;                   
                    foreach (var item in model.PurchaseItems)
                    {
                        DateTime expireDate;
                        var isValidExpireDate = DateTime.TryParseExact(item.ExpireDate, "MMMM dd yyyy", null, DateTimeStyles.None, out expireDate);
                        var purchaseItem = new PurchaseItem
                        {
                            Id = Guid.NewGuid().ToString(),
                            PurchaseId = purchase.Id,
                            ProductId = item.ProductId,
                            ExpireDate = isValidExpireDate ? expireDate : DateTime.Now.AddYears(2),
                            BatchNo = item.BatchNo,
                            PurchaseQuantity = item.PurchaseQuantity,
                            BonusQuantity = item.BonusQuantity,
                            PurchasePrice = item.PurchasePrice,
                            Amount = item.PurchaseQuantity * item.PurchasePrice,
                            CreatedOn = DateTime.Now,
                            BranchId = purchase.BranchId,
                            DiscountOnAmount = ((item.PurchaseQuantity * item.PurchasePrice) * purchase.Discount) / purchase.Subtotal
                        };
                       purchase.PurchaseItems.Add(purchaseItem);
                        //saving gift item
                        if (item.GiftItems.Any())
                        {
                            foreach (var obj in item.GiftItems)
                            {
                                var giftItem = new GifItem
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Name = obj.Name,
                                    Quantity = obj.Quantity,
                                    PurchaseItemId = purchaseItem.Id
                                };
                                purchaseItem.GifItems.Add(giftItem);
                            }
                        }
                        //saving stock item
                        if (model.PurchaseType == (int)PurchaseType.Received)
                        {
                            var discountPerItem = (purchaseItem.PurchasePrice * purchase.Discount) / purchase.Subtotal;
                            var additionalCostPerItem = (purchaseItem.PurchasePrice * purchase.AdditionalCost) / purchase.Subtotal;
                            for (int i = 0; i < purchaseItem.PurchaseQuantity; i++)
                            {
                                await _context.StockItems.AddAsync(new StockItem
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    ProductId = purchaseItem.ProductId,
                                    SupplierId = purchase.SupplierId,
                                    BranchId = purchase.BranchId,
                                    Quantity = 1,
                                    DiscountPerItem = discountPerItem,
                                    AdditionalCostPerItem = additionalCostPerItem,
                                    PurchaseRate = purchaseItem.PurchasePrice,
                                    PurchaseItemId = purchaseItem.Id,
                                    ExpiryDate = purchaseItem.ExpireDate,
                                    BatchNo = purchaseItem.BatchNo,
                                    CreatedOn = DateTime.Now,
                                    CreatedBy =currentUserId
                            });
                            }
                            if (purchaseItem.BonusQuantity > 0)
                            {
                                for (int i = 0; i < purchaseItem.BonusQuantity; i++)
                                {
                                    await _context.StockItems.AddAsync(new StockItem
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        ProductId = purchaseItem.ProductId,
                                        SupplierId = purchase.SupplierId,
                                        BranchId = purchase.BranchId,
                                        Quantity = 1,
                                        DiscountPerItem = 0,
                                        AdditionalCostPerItem = 0,
                                        PurchaseRate = 0,
                                        PurchaseItemId = purchaseItem.Id,
                                        ExpiryDate = purchaseItem.ExpireDate,
                                        BatchNo = purchaseItem.BatchNo,
                                        CreatedOn = DateTime.Now,
                                        CreatedBy = currentUserId
                                });
                                }
                            }
                        }
                    }
                   
                    if (isAccountsActive)
                    {
                        //party a/c credit
                        await _commonService.HitAccount(_context, purchase.BranchId, purchase.PurchaseNo, "Purchase",
                            dbSupplier.AccountId, 0, purchase.NetAmount,purchase.PurchaseDate);
                        //inventory a/c debit
                        await _commonService.HitAccount(_context, purchase.BranchId, purchase.PurchaseNo, "Purchase",
                           AccountHeadConstants.InventoryAC, purchase.NetAmount, 0, purchase.PurchaseDate);//inventory a/c id from database
                    }
                    else
                    {
                        //adjust supplier balance
                        purchase.SupplierBalance = dbSupplier.Balance + purchase.NetAmount;
                        dbSupplier.Balance += purchase.NetAmount;
                        _context.Suppliers.Update(dbSupplier);
                    }
                    await _context.Purchases.AddAsync(purchase);
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return await Result<string>.SuccessAsync(purchase.Id, "Purchase Added Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync(); 
                    return await Result<string>.FailAsync(exception.ToString(), exception.Message);
                }
            }
        }
            
        [Authorize(Policy = Permissions.Purchases.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        public virtual async Task<Result> Put([FromBody] PurchaseAddRequestModel model)
        {
            try
            {
                Purchase purchase = await _purchaseCommandService.FindAsync(model.Id);
                DateTime purchaseDate;
                var isValidDate = DateTime.TryParseExact(model.PurchaseDate, "dd/MM/yyyy", null, DateTimeStyles.None, out purchaseDate);
                var subtotal = CalculateSubtotalAndTotalVat(model.PurchaseItems, out decimal totalVat); 
                if (purchase != null)
                {
                    purchase.PurchaseDate = isValidDate ? purchaseDate : DateTime.UtcNow;
                    purchase.ReferenceNo = model.ReferenceNo;
                    purchase.TotalValue = subtotal - totalVat;
                    purchase.TotalVat = totalVat;
                    purchase.Subtotal = subtotal;
                    purchase.Discount = model.Discount;
                    purchase.AdditionalCost = model.AdditionalCost;
                    purchase.NetAmount = subtotal + model.AdditionalCost - model.Discount;
                    purchase.Remark = model.Remark;
                    purchase.UpdatedOn = DateTime.UtcNow;
                    purchase.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                }

                await _purchaseCommandService.UpdateAsync(purchase);
                //adjust supplier balance
                decimal diffNetAmount = model.NetAmount - purchase.NetAmount;
                var dbSupplier = await _supplierCommnadService.FindAsync(model.SupplierId);
                dbSupplier.Balance += diffNetAmount;
                await _supplierCommnadService.UpdateAsync(dbSupplier);
                //deleted items remove from database
                var modelIds = model.PurchaseItems.Select(x => x.Id).ToArray();
                var dbPurchaseItems =await _purchaseItemCommnadService.GetAllByPredicate(x=>x.PurchaseId==purchase.Id);
                var dbPurchaseItemIds = dbPurchaseItems.Select(x => x.Id).ToArray();
                dbPurchaseItemIds = dbPurchaseItemIds.Except(modelIds).ToArray();
                for (int i = 0; i < dbPurchaseItemIds.Length; i++)
                {
                    var dbPurchaseItem =await _purchaseItemCommnadService.FindAsync(dbPurchaseItemIds[i]);

                    if (dbPurchaseItem != null)
                    {
                      await  _stockItemCommnadService.DeleteAsync(x=>x.PurchaseItemId==dbPurchaseItem.Id);
                        // dbPurchaseItem.Deleted = true;
                        //await _purchaseItemCommnadService.UpdateAsync(dbPurchaseItem);
                        await _purchaseItemCommnadService.DeleteAsync(dbPurchaseItem.Id);
                    }
                }

                //updating purchase items
                foreach (var item in model.PurchaseItems)
                {
                    DateTime expireDate;
                    var isValidExpireDate = DateTime.TryParseExact(item.ExpireDate, "dd/MM/yyyy", null, DateTimeStyles.None, out expireDate);
                    var totalPurchaseItem = model.PurchaseItems.Sum(x => x.PurchaseQuantity);
                    var discountPerItem = purchase.Discount / totalPurchaseItem;
                    var additionalCostPerItem = purchase.AdditionalCost / totalPurchaseItem;

                    if (string.IsNullOrWhiteSpace(item.Id))
                    {
                        var purchaseItem = new PurchaseItem
                        {
                            Id = Guid.NewGuid().ToString(),
                            PurchaseId = purchase.Id,
                            ProductId = item.ProductId,
                            ExpireDate = isValidExpireDate ? expireDate : null,
                            BatchNo = item.BatchNo,
                            PurchaseQuantity = item.PurchaseQuantity,
                            BonusQuantity = item.BonusQuantity,
                            PurchasePrice = item.PurchasePrice,
                            Amount = item.PurchaseQuantity * item.PurchasePrice
                        };
                        await _purchaseItemCommnadService.AddAsync(purchaseItem);
                        //adjust stock item


                        if (model.PurchaseType == (int)PurchaseType.Received)
                        {
                            for (int i = 0; i < purchaseItem.PurchaseQuantity; i++)
                            {
                                 _context.StockItems.Add(new StockItem
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    ProductId = purchaseItem.ProductId,
                                    SupplierId = purchase.SupplierId,
                                    BranchId = purchase.BranchId,
                                    Quantity = 1,
                                    DiscountPerItem = discountPerItem,
                                    AdditionalCostPerItem = additionalCostPerItem,
                                    PurchaseRate = purchaseItem.PurchasePrice,
                                    PurchaseItemId = purchaseItem.Id,
                                    ExpiryDate = purchaseItem.ExpireDate,
                                    BatchNo = purchaseItem.BatchNo,
                                    CreatedOn = DateTime.UtcNow,
                                    CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId()
                                });
                            }
                            if (purchaseItem.BonusQuantity > 0)
                            {
                                for (int i = 0; i < purchaseItem.BonusQuantity; i++)
                                {
                                    _context.StockItems.Add(new StockItem
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        ProductId = purchaseItem.ProductId,
                                        SupplierId = purchase.SupplierId,
                                        BranchId = purchase.BranchId,
                                        Quantity = 1,
                                        DiscountPerItem = 0,
                                        AdditionalCostPerItem = 0,
                                        PurchaseRate = 0,
                                        PurchaseItemId = purchaseItem.Id,
                                        ExpiryDate = purchaseItem.ExpireDate,
                                        BatchNo = purchaseItem.BatchNo,
                                        CreatedOn = DateTime.UtcNow,
                                        CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId()
                                    });
                                }
                            }
                             _context.SaveChanges();

                        }
                    }

                    else
                    {
                        //update existing item stock
                        var purItem = await _purchaseItemCommnadService.FindAsync(item.Id);
                        var stockItemsByPurchaseItemId = await _stockItemCommnadService.GetAllByPredicate(x => x.PurchaseItemId == purItem.Id && x.PurchaseRate != 0);//bonus item should not be updated 
                        if (stockItemsByPurchaseItemId.Any())
                        {
                            foreach (var stockItem in stockItemsByPurchaseItemId)
                            {
                                stockItem.DiscountPerItem = discountPerItem;
                                stockItem.AdditionalCostPerItem = additionalCostPerItem;
                                _context.StockItems.Update(stockItem);
                                //await _stockItemCommnadService.UpdateAsync(stockItem);
                            }
                            _context.SaveChanges();
                        }


                    }
                }


                return await Result<string>.SuccessAsync(purchase.Id, "Purchase Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }

        }
        [Authorize(Policy = Permissions.Purchases.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpDelete]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                Purchase purchase = await _purchaseCommandService.FindAsync(id);
                if (purchase != null)
                {
                    purchase.Deleted = true;
                    purchase.UpdatedOn = DateTime.UtcNow;
                    purchase.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _purchaseCommandService.UpdateAsync(purchase);
                }
                return await Result<string>.SuccessAsync(id, "Purchase Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        [HttpPost]
        [Route("item-wise-purchase")]
        public virtual async Task<Result<Tuple<List<ItemPurchaseViewModel>, int>>> ItemWisePurchase(ItemPurchaseRequestModel request)
        {
            try
            {
                var query = _context.PurchaseItems.AsQueryable();
                if (!string.IsNullOrWhiteSpace(request.BranchId))
                    query = query.Where(x => x.BranchId == request.BranchId);
                //hardcoded initial value
                if (!request.FromDate.HasValue)
                    request.FromDate = DateTime.Now.AddYears(-5);
                if (!request.ToDate.HasValue)
                    request.ToDate = DateTime.Now;
                query = query.Where(x => x.CreatedOn.Date >= request.FromDate.Value.Date && x.CreatedOn.Date <= request.ToDate.Value.Date);
                var queryable = query.GroupBy(x => new { x.ProductId })
                          .Select(y => new
                          {
                              ProductId = y.Key.ProductId,
                              PurchaseQuantity = y.Sum(x => x.PurchaseQuantity),
                              BonusQuantity = y.Sum(x => x.BonusQuantity),
                              Subtotal = y.Sum(x => x.Amount),
                              Discount= y.Sum(x => x.DiscountOnAmount),
                              NetAmount = y.Sum(x => x.Amount) - y.Sum(x => x.DiscountOnAmount)
                          });
                
                if (!string.IsNullOrWhiteSpace(request.ProductId))
                    queryable = queryable.Where(x => x.ProductId == request.ProductId);

                var pQueryable = from purchaseItem in queryable
                                  join product in _context.Products on purchaseItem.ProductId equals product.Id
                                  join category in _context.Categories on product.CategoryId equals category.Id
                                  join supplier in _context.Suppliers on product.SupplierId equals supplier.Id
                                  select new ItemPurchaseViewModel()
                                  {
                                      ProductName = product.Name + " " + product.PackSize + " " + product.Category.Name,
                                      SupplierId = supplier.Id,
                                      SupplierName = supplier.Name,
                                      PurchaseQuantity = purchaseItem.PurchaseQuantity,
                                      BonusQuantity = purchaseItem.BonusQuantity,
                                      Subtotal = purchaseItem.Subtotal,
                                      Discount= purchaseItem.Discount,
                                      NetAmount = purchaseItem.NetAmount
                                  };

                if (!string.IsNullOrWhiteSpace(request.SupplierId))
                    pQueryable = pQueryable.Where(x => x.SupplierId == request.SupplierId);

                var count = pQueryable.Count();
                if (request.Page != -1)
                    pQueryable = pQueryable.Skip(request.Page * request.RowsPerPage).Take(request.RowsPerPage);
                var list = await pQueryable.ToListAsync();
                Tuple<List<ItemPurchaseViewModel>, int> content = new Tuple<List<ItemPurchaseViewModel>, int>(list, count);
                return await Result<Tuple<List<ItemPurchaseViewModel>, int>>.SuccessAsync(content, "Item Wise Purchase Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<ItemPurchaseViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }






        //utilities
        private decimal CalculateVat(decimal purchaseVat,decimal purchasePrice)
        {

            var perc = 1 + (purchaseVat / 100);
            var a = (purchasePrice / perc);
            return (purchasePrice - a);
        }
         private decimal CalculateSubtotalAndTotalVat(IList<PurchaseItemRequestModel> purchaseItems,out decimal totalVat)
        {
            totalVat = 0M;
            var subTotal = 0M;
            foreach (var item in purchaseItems)
            {
                var dbProduct = _context.Products.Find(item.ProductId);
                if (dbProduct == null) throw new NotFoundResultException("Product Not Found");
                totalVat += CalculateVat(dbProduct.PurchaseVat,(item.PurchasePrice*item.PurchaseQuantity));
                subTotal += (item.PurchaseQuantity * item.PurchasePrice);
            }
            return subTotal;
        }

    }
}
