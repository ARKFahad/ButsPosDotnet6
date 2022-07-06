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
using Microsoft.Extensions.Configuration;
using PbApp.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayReceiveController : ControllerBase
    {
        private readonly IBaseQueryService<PayReceive, PayReceiveRequestModel, PayReceiveViewModel> _payReceiveQueryService;
        private readonly IBaseCommandService<PayReceive> _payReceiveCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly GHP_POS_DBContext _context;
        private readonly IConfiguration _configuration;
        private readonly ICommonService _commonService;
        public PayReceiveController(IBaseQueryService<PayReceive, PayReceiveRequestModel, PayReceiveViewModel> payReceiveQueryService,
            IBaseCommandService<PayReceive> payReceiveCommandService,
            IHttpContextAccessor contextAccessor,
            GHP_POS_DBContext context,
            IConfiguration configuration,
            ICommonService commonService)
        {
            _payReceiveCommandService = payReceiveCommandService;
            _payReceiveQueryService = payReceiveQueryService;
            _contextAccessor = contextAccessor;
            _context = context;
            _configuration = configuration;
            _commonService = commonService;
        }

        [Authorize(Policy = Permissions.PayReceives.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<PayReceiveViewModel>, int>>> Search(PayReceiveRequestModel request)
        {
            try
            {
                Tuple<List<PayReceiveViewModel>, int> content = await _payReceiveQueryService.SearchAsync(request);
                return await Result<Tuple<List<PayReceiveViewModel>, int>>.SuccessAsync(content, "PayReceives Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<PayReceiveViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Route("voucher-no")]
        [HttpGet]
        public IActionResult GetReferenceNo()
        {
            return Ok(new { VoucherNo = _payReceiveCommandService.GetReferanceId(character: "V") });
        }
        [Authorize(Policy = Permissions.PayReceives.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] PayReceiveModel model)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var paymentMethod = await _context.PaymentMethods.FindAsync(model.PaymentMethodId);
                    var count = _context.PayReceives.Count() + 1;
                    PayReceive payReceive = new PayReceive();
                    DateTime voucherDate;
                    var isValidDate = DateTime.TryParseExact(model.VoucherDate, "dd/MM/yyyy", null, DateTimeStyles.None, out voucherDate);
                    payReceive.Id = Guid.NewGuid().ToString();
                    payReceive.Vnumber = "V" + count.ToString().PadLeft(7, '0');
                    payReceive.VoucherType = model.VoucherType;
                    payReceive.BranchId = model.BranchId;
                    payReceive.PaymentMethodId = model.PaymentMethodId;
                    payReceive.VoucherDate = isValidDate ? voucherDate : DateTime.Now;
                    payReceive.PaymentMethodId = model.PaymentMethodId;
                    payReceive.AccountId = model.AccountId;
                    payReceive.CheckNo = model.CheckNo;
                    payReceive.Amount = model.Amount;
                    payReceive.Description = model.Description;
                    payReceive.CreatedOn = DateTime.Now;
                    payReceive.UpdatedOn = DateTime.Now;
                    payReceive.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    payReceive.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _context.PayReceives.AddAsync(payReceive);

                    var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
                    if (isAccountsActive)
                    {
                        //pay
                        if (model.VoucherType == VoucherTypes.Pay)
                        {
                            //cash in hand/bank a/c credit
                            await _commonService.HitAccount(_context, payReceive.BranchId, payReceive.Vnumber, payReceive.Description,
                               paymentMethod.AccountId, 0, payReceive.Amount, payReceive.VoucherDate);


                            //selected a/c debit
                            await _commonService.HitAccount(_context, payReceive.BranchId, payReceive.Vnumber, payReceive.Description,
                              payReceive.AccountId, payReceive.Amount, 0, payReceive.VoucherDate);
                        }
                        //receive
                        else
                        {
                            //cash in hand/bank a/c debit
                            await _commonService.HitAccount(_context, payReceive.BranchId, payReceive.Vnumber, payReceive.Description,
                               paymentMethod.AccountId, payReceive.Amount, 0, payReceive.VoucherDate);


                            //selected a/c credit
                            await _commonService.HitAccount(_context, payReceive.BranchId, payReceive.Vnumber, payReceive.Description,
                              payReceive.AccountId, 0, payReceive.Amount, payReceive.VoucherDate);
                        }

                    }
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return await Result<string>.SuccessAsync(model.Id, "PayReceive Added Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return await Result<PayReceive>.FailAsync(exception.ToString(), exception.Message);
                }
            }
        }
    }
}
