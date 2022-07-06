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
using Microsoft.EntityFrameworkCore;
using PbApp.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IPdfService _pdfService;
        
        public StockController(GHP_POS_DBContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }
        [HttpGet]
        [Route("{sourceId}/{productId}")]
        public IActionResult StockCount(string sourceId, string productId)
        {
            var list = _context.StockItems.Where(x => x.ProductId == productId && x.BranchId == sourceId);
            return Ok(new { InStock = list.Sum(x => x.Quantity) });
        }



        [Authorize(Policy = Permissions.Stocks.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<StockModel>, int>>> Search(StockRequestModel request)
        {
            try
            {
              
                var query = _context.StockItems.GroupBy(x => new { x.ProductId, x.SupplierId ,x.BranchId})
                          .Select(y => new
                          {
                              ProductId = y.Key.ProductId,
                              SupplierId=y.Key.SupplierId,
                              BranchId=y.Key.BranchId,
                              Quantity = y.Sum(x => x.Quantity),
                              Price = (y.Sum(x => x.PurchaseRate)-y.Sum(x=>x.DiscountPerItem))+y.Sum(x=>x.AdditionalCostPerItem)
                          });
                var queryable = from stock in query
                             join product in _context.Products on stock.ProductId equals product.Id
                             join category in _context.Categories on product.CategoryId equals category.Id
                             join supplier in _context.Suppliers on stock.SupplierId equals supplier.Id
                             join branch in _context.Branches on stock.BranchId equals branch.Id
                             select new StockModel()
                             {
                                 BranchId=stock.BranchId,
                                 BranchName=branch.Name,
                                 ProductId=stock.ProductId,
                                 ProductName=product.Name,
                                 ProductPackSize=product.PackSize,
                                 CategoryId=category.Id,
                                 ProductCategory=category.Name,
                                 ProductBarcode=product.Barcode,
                                 Mrp=product.Mrp,
                                 SupplierId=stock.SupplierId,
                                 SupplierName=supplier.Name,
                                 Quantity=stock.Quantity,
                                 Value=stock.Price
                             };
                
                var expression = request.GetExpression();
                queryable = queryable.Where(expression);
                var count = queryable.Count();
                queryable = request.SkipAndTake(queryable);
                var list = await queryable.ToListAsync();
                Tuple<List<StockModel>, int> content = new Tuple<List<StockModel>, int>(list, count);
                return await Result<Tuple<List<StockModel>, int>>.SuccessAsync(content, "Stock Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<StockModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task <IActionResult> Print(StockRequestModel request)
        {
            try
            {
                request.IsAscending = true;

                var query = _context.StockItems.GroupBy(x => new { x.ProductId, x.SupplierId, x.BranchId })
                         .Select(y => new
                         {
                             ProductId = y.Key.ProductId,
                             SupplierId = y.Key.SupplierId,
                             BranchId = y.Key.BranchId,
                             Quantity = y.Sum(x => x.Quantity),
                             Price = (y.Sum(x => x.PurchaseRate) - y.Sum(x => x.DiscountPerItem)) + y.Sum(x => x.AdditionalCostPerItem)
                         });
                var queryable = from stock in query
                                join product in _context.Products on stock.ProductId equals product.Id
                                join category in _context.Categories on product.CategoryId equals category.Id
                                join supplier in _context.Suppliers on stock.SupplierId equals supplier.Id
                                join branch in _context.Branches on stock.BranchId equals branch.Id
                                select new StockModel()
                                {
                                    BranchId = stock.BranchId,
                                    BranchName = branch.Name,
                                    ProductId = stock.ProductId,
                                    ProductName = product.Name,
                                    ProductPackSize = product.PackSize,
                                    CategoryId = category.Id,
                                    ProductCategory = category.Name,
                                    ProductBarcode = product.Barcode,
                                    Mrp = product.Mrp,
                                    SupplierId = stock.SupplierId,
                                    Code=supplier.Code,
                                    SupplierName = supplier.Name,
                                    Quantity = stock.Quantity,
                                    Value = stock.Price
                                };
                var count = queryable.Count();
                var expression = request.GetExpression();
                queryable = queryable.Where(expression);
               
                var list = await queryable.ToListAsync();
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintStockReportToPdf(stream,_context.Branches.Find(request.BranchId), list, "Stock Report ");
                    bytes = stream.ToArray();
                    bytes = _pdfService.AddPageNumbers(bytes);
                }

                return File(bytes, MimeTypes.ApplicationPdf);

                // return await Result<Tuple<List<StockViewModel>, int>>.SuccessAsync(content, "Stock Found");
            }
            catch (Exception exception)
            {
                return  Ok(exception.ToString());
            }
        }

        [HttpPost]
        [Route("outofstock2")]
        public virtual async Task<Result<Tuple<List<StockModel>, int>>> OutOfStock(StockRequestModel request)
        {
            try
            {
                string[] stockProductIds = null;
                if (!string.IsNullOrWhiteSpace(request.BranchId))
                {
                    stockProductIds = _context.StockItems.GroupBy(x => new { x.ProductId, x.SupplierId, x.BranchId }).Where(x => x.Key.BranchId == request.BranchId)
                    .Select(y => y.Key.ProductId).ToArray();
                }
                else
                {
                    stockProductIds = _context.StockItems.GroupBy(x => new { x.ProductId })
                          .Select(y => y.Key.ProductId).ToArray();
                }


                var productIds = _context.Products.Where(x => x.Deleted == false).Select(x => x.Id).ToArray();

                var outOfStockProductIds = productIds.Except(stockProductIds).ToArray();

                List<StockModel> outOfStocks = new List<StockModel>();
                foreach (var productId in outOfStockProductIds)
                {
                    var product = await _context.Products.Include(x => x.Supplier).Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == productId);
                    outOfStocks.Add(new StockModel
                    {
                        BranchId = request.BranchId ?? null,
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ProductPackSize = product.PackSize,
                        CategoryId = product.Category?.Id,
                        ProductCategory = product.Category?.Name,
                        ProductBarcode = product.Barcode,
                        Mrp = product.Mrp,
                        SupplierId = product.SupplierId,
                        SupplierName = product.Supplier.Name,
                        Quantity = 0

                    });
                }
                var queryable = outOfStocks.AsQueryable();
                var count = outOfStocks.Count();
                var expression = request.GetExpression();
                queryable = queryable.Where(expression);
                queryable = request.SkipAndTake(queryable);
                var list = queryable.ToList();
                Tuple<List<StockModel>, int> content = new Tuple<List<StockModel>, int>(list, count);
                return await Result<Tuple<List<StockModel>, int>>.SuccessAsync(content, "Out of Stock Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<StockModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        [HttpPost]
        [Route("outofstock")]
        public virtual async Task<Result<Tuple<List<StockModel>, int>>> OutOfStockReport(StockRequestModel request)
        {
            try
            {
                List<StockModel> stocks = null;
                if (!string.IsNullOrWhiteSpace(request.BranchId))
                {
                  var   query = _context.StockItems.GroupBy(x => new { x.ProductId, x.SupplierId, x.BranchId }).Select(x=>new { 
                   ProductId=x.Key.ProductId,
                   SupplierId=x.Key.SupplierId,
                   BranchId=x.Key.BranchId
                  });
                    var queryabled = from stock in query
                                    join product in _context.Products on stock.ProductId equals product.Id
                                    join category in _context.Categories on product.CategoryId equals category.Id
                                    join supplier in _context.Suppliers on stock.SupplierId equals supplier.Id
                                    where stock.BranchId==request.BranchId
                                    select new StockModel()
                                    {
                                        BranchId = stock.BranchId,
                                        ProductId = stock.ProductId,
                                        ProductName = product.Name,
                                        ProductPackSize = product.PackSize,
                                        CategoryId = category.Id,
                                        ProductCategory = category.Name,
                                        ProductBarcode = product.Barcode,
                                        Mrp = product.Mrp,
                                        SupplierId = stock.SupplierId,
                                        SupplierName = supplier.Name,
                                        Quantity = 0
                                      
                                    };
                    stocks = queryabled.ToList();


                }
                else
                {
                    var query = _context.StockItems.GroupBy(x => new { x.ProductId,x.SupplierId }).Select(x => new {
                        ProductId = x.Key.ProductId,
                        SupplierId = x.Key.SupplierId,
                    });
                    var queryabled = from stock in query
                                     join product in _context.Products on stock.ProductId equals product.Id
                                     join category in _context.Categories on product.CategoryId equals category.Id
                                     join supplier in _context.Suppliers on stock.SupplierId equals supplier.Id
                                     select new StockModel()
                                     {
                                         ProductId = stock.ProductId,
                                         ProductName = product.Name,
                                         ProductPackSize = product.PackSize,
                                         CategoryId = category.Id,
                                         ProductCategory = category.Name,
                                         ProductBarcode = product.Barcode,
                                         Mrp = product.Mrp,
                                         SupplierId = stock.SupplierId,
                                         SupplierName = supplier.Name,
                                         Quantity = 0

                                     };
                    stocks = queryabled.ToList();
                }


                var products = _context.Products.Include(x => x.Supplier).Include(x => x.Category).Where(x => x.Deleted == false).Select(x =>
                new StockModel()
                {
                    BranchId = request.BranchId ?? null,
                    ProductId = x.Id,
                    ProductName = x.Name,
                    ProductPackSize = x.PackSize,
                    CategoryId = x.CategoryId,
                    ProductCategory = x.Category.Name,
                    ProductBarcode = x.Barcode,
                    Mrp = x.Mrp,
                    SupplierId = x.SupplierId,
                    SupplierName = x.Supplier.Name,
                    Quantity = 0

                }).ToList();

                var outOfStocks = products.Except(stocks, new StockComparer());

             
                var queryable = outOfStocks.AsQueryable();
                var count = outOfStocks.Count();
                var expression = request.GetExpression();
                queryable = queryable.Where(expression);
                queryable = request.SkipAndTake(queryable);
                var list = queryable.ToList();
                Tuple<List<StockModel>, int> content = new Tuple<List<StockModel>, int>(list, count);
                return await Result<Tuple<List<StockModel>, int>>.SuccessAsync(content, "Out of Stock Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<StockModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpPost]
            [Route("outofstock/print")]
            public virtual  IActionResult OutOfStockPrint(StockRequestModel request)
            {
                try
                {
                request.IsAscending = true;
                List<StockModel> stocks = null;
                if (!string.IsNullOrWhiteSpace(request.BranchId))
                {
                    var query = _context.StockItems.GroupBy(x => new { x.ProductId, x.SupplierId, x.BranchId }).Select(x => new {
                        ProductId = x.Key.ProductId,
                        SupplierId = x.Key.SupplierId,
                        BranchId = x.Key.BranchId
                    });
                    var queryabled = from stock in query
                                     join product in _context.Products on stock.ProductId equals product.Id
                                     join category in _context.Categories on product.CategoryId equals category.Id
                                     join supplier in _context.Suppliers on stock.SupplierId equals supplier.Id
                                     where stock.BranchId == request.BranchId
                                     select new StockModel()
                                     {
                                         BranchId = stock.BranchId,
                                         ProductId = stock.ProductId,
                                         ProductName = product.Name,
                                         ProductPackSize = product.PackSize,
                                         CategoryId = category.Id,
                                         ProductCategory = category.Name,
                                         ProductBarcode = product.Barcode,
                                         Mrp = product.Mrp,
                                         SupplierId = stock.SupplierId,
                                         SupplierName = supplier.Name,
                                         Quantity = 0

                                     };
                    stocks = queryabled.ToList();


                }
                else
                {
                    var query = _context.StockItems.GroupBy(x => new { x.ProductId, x.SupplierId }).Select(x => new {
                        ProductId = x.Key.ProductId,
                        SupplierId = x.Key.SupplierId,
                    });
                    var queryabled = from stock in query
                                     join product in _context.Products on stock.ProductId equals product.Id
                                     join category in _context.Categories on product.CategoryId equals category.Id
                                     join supplier in _context.Suppliers on stock.SupplierId equals supplier.Id
                                     select new StockModel()
                                     {
                                         ProductId = stock.ProductId,
                                         ProductName = product.Name,
                                         ProductPackSize = product.PackSize,
                                         CategoryId = category.Id,
                                         ProductCategory = category.Name,
                                         ProductBarcode = product.Barcode,
                                         Mrp = product.Mrp,
                                         SupplierId = stock.SupplierId,
                                         SupplierName = supplier.Name,
                                         Quantity = 0

                                     };
                    stocks = queryabled.ToList();
                }


                var products = _context.Products.Include(x => x.Supplier).Include(x => x.Category).Where(x => x.Deleted == false).Select(x =>
                new StockModel()
                {
                    BranchId = request.BranchId ?? null,
                    ProductId = x.Id,
                    ProductName = x.Name,
                    ProductPackSize = x.PackSize,
                    CategoryId = x.CategoryId,
                    ProductCategory = x.Category.Name,
                    ProductBarcode = x.Barcode,
                    Mrp = x.Mrp,
                    SupplierId = x.SupplierId,
                    SupplierName = x.Supplier.Name,
                    Quantity = 0

                }).ToList();

                var outOfStocks = products.Except(stocks, new StockComparer()).ToList();

                byte[] bytes;
                    using (var stream = new MemoryStream())
                    {
                        _pdfService.PrintOutOfStockReportToPdf(stream, _context.Branches.Find(request.BranchId), outOfStocks, "OutOfStock Report ");
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
public class StockComparer : IEqualityComparer<StockModel>
{
    public bool Equals(StockModel x, StockModel y)
    {
        if (x.ProductId == y.ProductId)
            return true;

        return false;
    }

    public int GetHashCode(StockModel obj)
    {
        return obj.ProductId.GetHashCode();
    }
}