using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
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
using Microsoft.Extensions.Configuration;
using PbApp.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferMultipleController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IBaseQueryService<TransferMultiple, TransferMultipleRequestModel, TransferMultipleViewModel> _transferMultipleQueryService;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;

        public TransferMultipleController(GHP_POS_DBContext context, IHttpContextAccessor contextAccessor,
            IBaseQueryService<TransferMultiple, TransferMultipleRequestModel, TransferMultipleViewModel> transferMultipleQueryService,
            ICommonService commonService,
            IConfiguration configuration)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _transferMultipleQueryService = transferMultipleQueryService;
            _commonService = commonService;
            _configuration = configuration;

        }
        [Authorize(Policy = Permissions.Transfers.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> TransferMultipleItem(TransferMultipleAddRequestModel transferMultipleModel)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var destinationBranch = await _context.Branches.FindAsync(transferMultipleModel.DestinationId);
                    var sourceBranch = await _context.Branches.FindAsync(transferMultipleModel.SourceId);
                    var dbSupplier = await _context.Suppliers.Where(x=>x.AccountId==sourceBranch.SupplierAccountId)?.FirstOrDefaultAsync();
                    if (dbSupplier == null) throw new NotFoundResultException("Please Make Source Branch as Supplier");

                    var totalTransferAmount = 0M;
                    var count = _context.TransferMultiples.Count() + 1;
                     TransferMultiple transferMultiple = new TransferMultiple
                        {
                            Id = Guid.NewGuid().ToString(),
                            TransferNo= "T" + count.ToString().PadLeft(7, '0'),
                            SourceId = transferMultipleModel.SourceId,
                            DestinationId = transferMultipleModel.DestinationId,
                         //  SupplierBalance = dbSupplier.Balance + totalTransferAmount,
                            CreatedOn = DateTime.Now,
                            CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId() ?? "",
                            UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId() ?? "",
                            UpdatedOn = DateTime.Now

                        };
                    
                    foreach (var transferDetail in transferMultipleModel.TransferDetails)
                    {
                        var itemFromSorce = await _context.StockItems.Where(x => x.BranchId == transferMultipleModel.SourceId && x.ProductId == transferDetail.ProductId).OrderBy(x=>x.CreatedOn).Take(transferDetail.TransferQty).ToListAsync();
                        if (itemFromSorce == null)
                            throw new NotFoundResultException("Selected Product Not Found In Source Branch");
                        var totalPurchasePrice = 0M;
                        foreach (var item in itemFromSorce)
                        {
                            totalPurchasePrice += ((item.PurchaseRate + item.AdditionalCostPerItem) - item.DiscountPerItem);
                            item.BranchId = transferMultipleModel.DestinationId;
                        }
                        totalTransferAmount+=totalPurchasePrice;
                        //stock check
                        var stockCount = _context.StockItems.Where(x => x.ProductId == transferDetail.ProductId && x.BranchId == transferMultipleModel.SourceId).Sum(x => x.Quantity);
                        if (transferDetail.TransferQty > stockCount) throw new Exception("Out of Stock");
                        _context.UpdateRange(itemFromSorce);
                        await _context.SaveChangesAsync();
                        var td = new TransferDetail
                        {
                            Id = Guid.NewGuid().ToString(),
                            TransferMultipleId = transferMultiple.Id,
                            ProductId = transferDetail.ProductId,
                            TransferQty = transferDetail.TransferQty,
                            Amount = totalPurchasePrice,
                            CreatedOn = DateTime.Now,
                        };
                        transferMultiple.TransferDetails.Add(td);

                    }
                    //accounts entry

                    var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
                    if (isAccountsActive)
                    {
                        //inventory a/c debit for Destination Branch
                        await _commonService.HitAccount(_context, transferMultiple.DestinationId, transferMultiple.TransferNo, "TransferMultiple",
                           AccountHeadConstants.InventoryAC, totalTransferAmount, 0, transferMultiple.CreatedOn);
                        //inventory a/c credit for Source Branch
                        await _commonService.HitAccount(_context, transferMultiple.SourceId, transferMultiple.TransferNo, "TransferMultiple",
                           AccountHeadConstants.InventoryAC, 0, totalTransferAmount, transferMultiple.CreatedOn);

                        //source branch as supplier a/c credit
                        await _commonService.HitAccount(_context, transferMultiple.DestinationId, transferMultiple.TransferNo, "TransferMultiple",
                           sourceBranch.SupplierAccountId, 0, totalTransferAmount, transferMultiple.CreatedOn);
                        //destination branch supplier A/C debit
                        await _commonService.HitAccount(_context, transferMultiple.SourceId, transferMultiple.TransferNo, "TransferMultiple",
                           destinationBranch.SupplierAccountId, totalTransferAmount, 0, transferMultiple.CreatedOn);
                    }
                    else
                    {
                        //adjust supplier balance

                        dbSupplier.Balance += totalTransferAmount;
                        _context.Suppliers.Update(dbSupplier);
                    }
                    transferMultiple.TotalAmount = totalTransferAmount;
                    await _context.TransferMultiples.AddAsync(transferMultiple);
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return Ok(new {Succeeded=true, Message="TransferMultiple Successfull" });
                
                   
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    return Ok(new { Succeeded = true, Message = ex.Message });
                }
            }
           
        }



        [Authorize(Policy = Permissions.Transfers.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<TransferMultipleViewModel>, int>>> Search(TransferMultipleRequestModel request)
        {
            try
            {
                Tuple<List<TransferMultipleViewModel>, int> content = await _transferMultipleQueryService.SearchAsync(request);
                return await Result<Tuple<List<TransferMultipleViewModel>, int>>.SuccessAsync(content, "TransferMultiple Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<TransferMultipleViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }




    }
}
