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
    public class TransferController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IBaseQueryService<TransferHistory, TransferRequestModel, TransferViewModel> _transferQueryService;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;

        public TransferController(GHP_POS_DBContext context, IHttpContextAccessor contextAccessor,
            IBaseQueryService<TransferHistory, TransferRequestModel, TransferViewModel> transferQueryService,
            ICommonService commonService,
            IConfiguration configuration)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _transferQueryService = transferQueryService;
            _commonService = commonService;
            _configuration = configuration;

        }
        [Authorize(Policy = Permissions.Transfers.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> TransferItem(TransferModel transfermodel)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var destinationBranch = await _context.Branches.FindAsync(transfermodel.DestinationId);
                    var sourceBranch = await _context.Branches.FindAsync(transfermodel.SourceId);
                    var dbSupplier = await _context.Suppliers.Where(x=>x.AccountId==sourceBranch.SupplierAccountId)?.FirstOrDefaultAsync();
                    if (dbSupplier == null) throw new NotFoundResultException("Please Make Source Branch as Supplier");
                    var itemFromSorce = await _context.StockItems.Where(x => x.BranchId == transfermodel.SourceId && x.ProductId == transfermodel.ProductId).Take(transfermodel.TransferQty).ToListAsync();
                    if (itemFromSorce==null)
                        throw new NotFoundResultException("Selected Product Not Found In Source Branch");
                    var totalPurchasePrice = 0M;
                        foreach (var item in itemFromSorce)
                        {
                             totalPurchasePrice += ((item.PurchaseRate+item.AdditionalCostPerItem)-item.DiscountPerItem);
                             item.BranchId = transfermodel.DestinationId;
                        }

                    var count = _context.TransferHistories.Count() + 1;
                    TransferHistory transferHistory = new TransferHistory
                        {
                            Id = Guid.NewGuid().ToString(),
                            TransferNo= "T" + count.ToString().PadLeft(7, '0'),
                            SourceId = transfermodel.SourceId,
                            ProductId = transfermodel.ProductId,
                            DestinationId = transfermodel.DestinationId,
                            Quantity = transfermodel.TransferQty,
                            Amount=totalPurchasePrice,
                          //  SupplierBalance = dbSupplier.Balance + totalPurchasePrice,
                            CreatedOn = DateTime.Now,
                            CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId() ?? "",
                            UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId() ?? "",
                            UpdatedOn = DateTime.Now

                        };
                       
                      
                       //stock check
                        var stockCount = _context.StockItems.Where(x => x.ProductId == transfermodel.ProductId && x.BranchId == transfermodel.SourceId).Sum(x => x.Quantity);
                        if (transfermodel.TransferQty > stockCount) throw new Exception("Out of Stock");
                    _context.UpdateRange(itemFromSorce);
                    await _context.SaveChangesAsync();
                    await _context.AddAsync(transferHistory);
                   
                    //accounts entry

                    var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
                    if (isAccountsActive)
                    {
                        //inventory a/c debit for Destination Branch
                        await _commonService.HitAccount(_context, transferHistory.DestinationId, transferHistory.TransferNo, "Transfer",
                           AccountHeadConstants.InventoryAC, transferHistory.Amount, 0,transferHistory.CreatedOn);
                        //inventory a/c credit for Source Branch
                        await _commonService.HitAccount(_context, transferHistory.SourceId, transferHistory.TransferNo, "Transfer",
                           AccountHeadConstants.InventoryAC, 0, transferHistory.Amount, transferHistory.CreatedOn);

                        //source branch as supplier a/c credit
                        await _commonService.HitAccount(_context, transferHistory.DestinationId, transferHistory.TransferNo, "Transfer",
                           sourceBranch.SupplierAccountId, 0, transferHistory.Amount, transferHistory.CreatedOn);
                        //destination branch supplier A/C debit
                        await _commonService.HitAccount(_context, transferHistory.SourceId, transferHistory.TransferNo, "Transfer",
                           destinationBranch.SupplierAccountId, transferHistory.Amount,0, transferHistory.CreatedOn);
                    }
                    else
                    {
                        //adjust supplier balance

                        dbSupplier.Balance += transferHistory.Amount;
                        _context.Suppliers.Update(dbSupplier);
                    }


                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return Ok(new {Succeeded=true, Message="Transfer Successfull" });
                
                   
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
        public virtual async Task<Result<Tuple<List<TransferViewModel>, int>>> Search(TransferRequestModel request)
        {
            try
            {
                Tuple<List<TransferViewModel>, int> content = await _transferQueryService.SearchAsync(request);
                return await Result<Tuple<List<TransferViewModel>, int>>.SuccessAsync(content, "Transfer Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<TransferViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }

    }
}
