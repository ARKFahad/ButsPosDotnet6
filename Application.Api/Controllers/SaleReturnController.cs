using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleReturnController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IPdfService _pdfService;
        private readonly IBaseQueryService<SaleReturn, SaleReturnRequestModel, SaleReturnViewModel> _saleReturnQueryService;
        private readonly IConfiguration _configuration;
        private readonly ICommonService _commonService;
        public SaleReturnController(
           GHP_POS_DBContext context, IHttpContextAccessor contextAccessor, IPdfService pdfService, 
           IBaseQueryService<SaleReturn, SaleReturnRequestModel, SaleReturnViewModel> saleReturnQueryService,
            IConfiguration configuration,
            ICommonService commonService)
        {

            _context = context;
            _contextAccessor = contextAccessor;
            _pdfService = pdfService;
            _saleReturnQueryService = saleReturnQueryService;
            _configuration = configuration;
            _commonService = commonService;
        }
        [Authorize(Policy = Permissions.SaleReturns.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<SaleReturnViewModel>, int>>> Search(SaleReturnRequestModel request)
        {
            try
            {
                Tuple<List<SaleReturnViewModel>, int> content = await _saleReturnQueryService.SearchAsync(request);
                return await Result<Tuple<List<SaleReturnViewModel>, int>>.SuccessAsync(content, "Sale Return Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<SaleReturnViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.SaleReturns.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(SaleReturnRequestModel request)
        {
            try
            {
                request.Page = -1;
                request.IsAscending = true;
                Tuple<List<SaleReturnViewModel>, int> content = await _saleReturnQueryService.SearchAsync(request);
                byte[] bytes;

                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintSaleReturnReportToPdf(stream, content.Item1, "Supplier Payment Report ", _context.Branches.Find(request.BranchId), request.fromDate, request.ToDate);
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
        [Route("returnItem")]
        public async Task<IActionResult> ReturnSoldItem(SaleReturnModel model)
        {
            var referenceNo = "";
            var totalItem = 0;
            decimal totalValue = 0M;
            decimal totalCogsOfReturnItems = 0M;

            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var dbCustomer = await _context.Customers.FindAsync(model.CustomerId);
                    var sale = _context.Sales.Find(model.Id);
                    foreach (var item in model.SaleItems)
                    {
                        var saleItem = _context.SaleItems.Find(item.Id);
                        referenceNo = "Return-"+model.SaleNo;
                        totalItem += item.ReturnQty;
                        totalValue += (item.Price * item.ReturnQty);
                        totalCogsOfReturnItems += (item.ReturnQty * saleItem.CogsPerItem);
                        var modifiedQuantity = saleItem.Quantity - item.ReturnQty;
                        if (modifiedQuantity < 0)
                            throw new Exception("Return Failed Due To Invalid Quantity");
                        saleItem.ReturnQuantity = item.ReturnQty;
                        _context.SaleItems.Update(saleItem);
                        for (var i = 1; i <= item.ReturnQty; i++)
                        {  
                            await _context.StockItems.AddAsync(new StockItem
                            {
                            Id = Guid.NewGuid().ToString(),
                            ProductId = item.ProductId,
                            Quantity = 1,
                            PurchaseRate = item.ItemCogs,
                            BranchId = sale.BranchId,
                            CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId(),
                            SupplierId = saleItem.SupplierId
                        });
                        }
                    }
                    var discount = (totalValue * sale.DiscountInPercentage) / 100;
                    var totalValueWithoutDiscount = totalValue - discount;
                    SaleReturn saleReturn = new SaleReturn();
                    saleReturn.Id = Guid.NewGuid().ToString();
                    saleReturn.CustomerId = model.CustomerId;
                    saleReturn.ReferenceNo = referenceNo;
                    saleReturn.BranchId = model.BranchId;
                    saleReturn.TotalItem = totalItem;
                    saleReturn.Discount=discount;
                    saleReturn.TotalValue = totalValueWithoutDiscount;
                    saleReturn.TotalCogsOfReturnItems = totalCogsOfReturnItems;
                    saleReturn.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    saleReturn.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    saleReturn.SaleReturnDate = DateTime.Now;
                    saleReturn.CreatedOn = DateTime.Now;
                    saleReturn.UpdatedOn = DateTime.Now;
                    await _context.SaleReturns.AddAsync(saleReturn);
                    foreach (var item in model.SaleItems)
                    {
                        if (item.ReturnQty > 0)
                        {
                            SaleReturnItem saleReturnItem = new SaleReturnItem();
                            saleReturnItem.Id = Guid.NewGuid().ToString();
                            saleReturnItem.SaleReturnId = saleReturn.Id;
                            saleReturnItem.ProductId = item.ProductId;
                            saleReturnItem.ReturnQuantity = item.ReturnQty;
                            saleReturnItem.Rate = item.Price;
                            saleReturnItem.Amount = (item.Price * item.ReturnQty);
                            saleReturnItem.Reason = model.Reason;
                            saleReturnItem.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                            saleReturnItem.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                            saleReturnItem.CreatedOn = DateTime.Now;
                            saleReturnItem.UpdatedOn = DateTime.Now;
                            await _context.SaleReturnItems.AddAsync(saleReturnItem);
                          
                        }                     
                    }
                    sale.TotalSaleReturnedAmount += saleReturn.TotalValue;                  
                    var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
                    if (isAccountsActive)
                    {
                      //  if (sale.DueAmount > 0)
                      //  {
                        //    sale.CustomerBalance -= totalValue;
                            //customer a/c credit
                          //  await _commonService.HitAccount(_context, saleReturn.BranchId, saleReturn.ReferenceNo, "Sale Return",
                            //  dbCustomer.AccountId, 0, saleReturn.TotalValue, saleReturn.CreatedOn);
                      //  }
                       // else
                      //  {
                            //cash a/c credit
                            await _commonService.HitAccount(_context, saleReturn.BranchId, saleReturn.ReferenceNo, "Sale Return",
                               AccountHeadConstants.CashInHand, 0, saleReturn.TotalValue, saleReturn.CreatedOn);
                       // }
                            //sale a/c debit
                            await _commonService.HitAccount(_context, saleReturn.BranchId, saleReturn.ReferenceNo, "Sale Return",
                               AccountHeadConstants.Sales, saleReturn.TotalValue,0,saleReturn.CreatedOn);
                            
                            //inventory a/c debit
                            await _commonService.HitAccount(_context, saleReturn.BranchId, saleReturn.ReferenceNo, "Sale Return",
                                AccountHeadConstants.InventoryAC, saleReturn.TotalCogsOfReturnItems, 0, saleReturn.CreatedOn);
                            //cogs a/c credit
                            await _commonService.HitAccount(_context, saleReturn.BranchId, saleReturn.ReferenceNo, "Sale Return",
                               AccountHeadConstants.COGS, 0, saleReturn.TotalCogsOfReturnItems, saleReturn.CreatedOn);                       
                    }

                    _context.Sales.Update(sale);
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();

                    return Ok(new { Message = "Return Item Successfully", Succeeded=true });
                }
                catch (Exception e)
                {
                    await dbContextTransaction.RollbackAsync();
                    return Ok(new { SaleId = "", Message = e.Message ,Succeeded=false});  
                }
            }
        }

    }
}
