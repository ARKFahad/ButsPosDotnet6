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
    public class DamageLostController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IBaseQueryService<DamageLost, DamageLostRequestModel, DamageLostViewModel> _damageLostQueryService;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;

        public DamageLostController(GHP_POS_DBContext context, IHttpContextAccessor contextAccessor,
            IBaseQueryService<DamageLost, DamageLostRequestModel, DamageLostViewModel> damageLostQueryService,
            ICommonService commonService,
            IConfiguration configuration)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _damageLostQueryService = damageLostQueryService;
            _commonService = commonService;
            _configuration = configuration;

        }
        [Authorize(Policy = Permissions.DamageLosts.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> AddDamageLost(DamageLost damageLostmodel)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                  
                    var itemsFromBranch = await _context.StockItems.Where(x => x.BranchId == damageLostmodel.BranchId && x.ProductId == damageLostmodel.ProductId).Take(damageLostmodel.DamageLostQuantity).ToListAsync();
                    if (!itemsFromBranch.Any())
                        throw new NotFoundResultException("Product Not Found In Selected Branch");
                    var totalPurchasePrice = 0M;
                    foreach (var item in itemsFromBranch)
                    {
                        totalPurchasePrice += ((item.PurchaseRate + item.AdditionalCostPerItem) - item.DiscountPerItem);
                    }
                    var count = _context.DamageLosts.Count() + 1;

                    damageLostmodel.Id = Guid.NewGuid().ToString();
                    damageLostmodel.Code = "DL" + count.ToString().PadLeft(7, '0');
                    damageLostmodel.DamageLostAmount = totalPurchasePrice;
                    damageLostmodel.CreatedOn = DateTime.Now;
                    damageLostmodel.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    damageLostmodel.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    damageLostmodel.UpdatedOn = DateTime.Now;
                        
                       
                      
                       //stock check
                        var stockCount = _context.StockItems.Where(x => x.ProductId == damageLostmodel.ProductId && x.BranchId == damageLostmodel.BranchId).Sum(x => x.Quantity);
                        if (damageLostmodel.DamageLostQuantity > stockCount) throw new Exception("Out of Stock");
                    _context.RemoveRange(itemsFromBranch);
                    await _context.SaveChangesAsync();
                    await _context.AddAsync(damageLostmodel);
                   
                    //accounts entry

                    var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
                    if (isAccountsActive)
                    {
                        //inventory a/c credit 
                        await _commonService.HitAccount(_context, damageLostmodel.BranchId, damageLostmodel.Code, "Damage or Lost",
                           AccountHeadConstants.InventoryAC,0, damageLostmodel.DamageLostAmount, damageLostmodel.CreatedOn);
                        //damage lost account debit
                        await _commonService.HitAccount(_context, damageLostmodel.BranchId, damageLostmodel.Code, "Damage or Lost",
                           AccountHeadConstants.DamageLostGoods, damageLostmodel.DamageLostAmount,0, damageLostmodel.CreatedOn);

                    }


                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return Ok(new {Succeeded=true, Message="Damage Lost has been added Successfully" });
                
                   
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    return Ok(new { Succeeded = true, Message = ex.Message });
                }
            }
           
        }
        [Authorize(Policy = Permissions.DamageLosts.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<DamageLostViewModel>, int>>> Search(DamageLostRequestModel request)
        {
            try
            {
                Tuple<List<DamageLostViewModel>, int> content = await _damageLostQueryService.SearchAsync(request);
                return await Result<Tuple<List<DamageLostViewModel>, int>>.SuccessAsync(content, "DamageLost Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<DamageLostViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }

    }
}
