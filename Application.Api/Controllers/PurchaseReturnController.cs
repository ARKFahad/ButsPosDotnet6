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
using Microsoft.Extensions.Configuration;
using PbApp.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseReturnController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IPdfService _pdfService;
        private readonly IBaseQueryService<PurchaseReturn, PurchaseReturnRequestModel, PurchaseReturnViewModel> _purchaseReturnQueryService;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;
        public PurchaseReturnController(
          GHP_POS_DBContext context, IHttpContextAccessor contextAccessor,
          IPdfService pdfService, 
          IBaseQueryService<PurchaseReturn, PurchaseReturnRequestModel, PurchaseReturnViewModel> purchaseReturnQueryService,
           ICommonService commonService,
           IConfiguration configuration)
        {

            _context = context;
            _contextAccessor = contextAccessor;
            _pdfService = pdfService;
            _purchaseReturnQueryService = purchaseReturnQueryService;
            _commonService = commonService;
            _configuration = configuration;
        }
        [Authorize(Policy = Permissions.PurchaseReturns.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<PurchaseReturnViewModel>, int>>> Search(PurchaseReturnRequestModel request)
        {
            try
            {
                Tuple<List<PurchaseReturnViewModel>, int> content = await _purchaseReturnQueryService.SearchAsync(request);
                return await Result<Tuple<List<PurchaseReturnViewModel>, int>>.SuccessAsync(content, "Purcahse Return Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<PurchaseReturnViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.PurchaseReturns.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("returnItem")]
        public async Task<IActionResult> ReturnPurchaseItem(PurchaseReturnModel model)
        {
            var referenceNo = "";
            var totalItem = 0;
            decimal totalValue = 0M;

            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var purchasetableRow =await _context.Purchases.Include(x=>x.PurchaseItems).Where(x=>x.Id==model.Id).FirstOrDefaultAsync();
                    var totalPurchaseItem = purchasetableRow.PurchaseItems.Sum(x => x.PurchaseQuantity);
                   
                        var discountPerItem = purchasetableRow.Discount / totalPurchaseItem;
                        var additionalCostPerItem = purchasetableRow.AdditionalCost / totalPurchaseItem;
                        foreach (var item in model.PurchaseItems)
                    {
                        referenceNo = "Return-"+model.PurchaseNo;
                        totalItem += item.ReturnQty;
                        totalValue += Math.Round(((item.PurchasePrice-discountPerItem+additionalCostPerItem) * item.ReturnQty),2);

                        var purchaseItem = _context.PurchaseItems.Find(item.Id);
                        var modifiedQuantity = purchaseItem.PurchaseQuantity - item.ReturnQty;
                        if (modifiedQuantity < 0)
                            throw new Exception("Return Failed Due To Invalid Quantity");

                        purchaseItem.PurchaseQuantity = purchaseItem.PurchaseQuantity;
                        purchaseItem.ReturnQuantity += item.ReturnQty;
                        purchaseItem.PurchasePrice = item.PurchasePrice;
                        purchaseItem.Amount = purchaseItem.Amount - (item.PurchasePrice * item.ReturnQty);
                        purchasetableRow.Subtotal -= (item.PurchasePrice * item.ReturnQty);
                        purchasetableRow.NetAmount -= (item.PurchasePrice * item.ReturnQty);

                        _context.Update(purchaseItem);


                        var itemToremoveFromStock = _context.StockItems.Where(x => x.ProductId == item.ProductId&& x.BranchId==model.BranchId).OrderBy(x=>x.CreatedOn).Take(item.ReturnQty).ToList();


                        _context.RemoveRange(itemToremoveFromStock);
                       await _context.SaveChangesAsync();


                    

                    }
                    _context.Update(purchasetableRow);
                    await _context.SaveChangesAsync();
                    PurchaseReturn purchasereturn = new PurchaseReturn();
                    purchasereturn.Id = Guid.NewGuid().ToString();
                    purchasereturn.SupplierId = model.SupplierId;
                    purchasereturn.ReferenceNo = referenceNo;
                    purchasereturn.BranchId = model.BranchId;
                    purchasereturn.TotalItem = totalItem;
                    purchasereturn.TotalValue = totalValue;
                    purchasereturn.CreatedOn = DateTime.Now;
                    purchasereturn.UpdatedOn = DateTime.Now;
                    purchasereturn.CreatedBy = purchasetableRow.CreatedBy;
                    purchasereturn.UpdatedBy = purchasetableRow.UpdatedBy;
                    await _context.AddAsync(purchasereturn);
                    await _context.SaveChangesAsync();

                    foreach (var item in model.PurchaseItems)
                    {
                        if (item.ReturnQty > 0)
                        {
                            PurchaseReturnItem purchaseReturnItem = new PurchaseReturnItem();
                            purchaseReturnItem.Id = Guid.NewGuid().ToString();
                            purchaseReturnItem.PurchaseReturnId = purchasereturn.Id;
                            purchaseReturnItem.ProductId = item.ProductId;
                            purchaseReturnItem.ReturnQuantity = item.ReturnQty;
                            purchaseReturnItem.Rate = item.PurchasePrice;
                            purchaseReturnItem.Amount = (item.PurchasePrice * item.ReturnQty);
                            purchaseReturnItem.Reason = model.Reason;
                            

                            await _context.AddAsync(purchaseReturnItem);
                            await _context.SaveChangesAsync();
                        }
                      

                    }
                    //Supplier Balance Update
                    var dbSupplier = await _context.Suppliers.FindAsync(model.SupplierId);
                    dbSupplier.Balance -= totalValue;
                    _context.Update(dbSupplier);
                    await _context.SaveChangesAsync();

                    //adjust Supplier balance
                    purchasetableRow.SupplierBalance -= totalValue;
                    _context.Purchases.Update(purchasetableRow);
                    

                    var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
                    if (isAccountsActive)
                    {
                        //party a/c debit
                        await _commonService.HitAccount(_context, purchasereturn.BranchId, purchasereturn.ReferenceNo, "Purchase Return",
                            dbSupplier.AccountId,purchasereturn.TotalValue,0,purchasereturn.CreatedOn);
                        //inventory a/c credit
                        await _commonService.HitAccount(_context, purchasereturn.BranchId, purchasereturn.ReferenceNo, "Purchase Return",
                           AccountHeadConstants.InventoryAC, 0, purchasereturn.TotalValue, purchasereturn.CreatedOn);//inventory a/c id from database
                    }
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();

                    return Ok(new { Message = "Return Item Successfully", Succeeded = true });
                }
                catch (Exception e)
                {
                    await dbContextTransaction.RollbackAsync();
                    return Ok(new { PurchaseId = "", Message = e.Message, Succeeded = false });
                }
            }
        }

        }
}
