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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private readonly IBaseQueryService<Expense, ExpenseRequestModel, ExpenseViewModel> _expenseQueryService;
        private readonly IBaseCommandService<Expense> _expenseCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly GHP_POS_DBContext _context;
        private readonly IPdfService _pdfService;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;
        public ExpenseController(IBaseQueryService<Expense, ExpenseRequestModel, ExpenseViewModel> expenseQueryService,
            IBaseCommandService<Expense> expenseCommandService, GHP_POS_DBContext context,
            IHttpContextAccessor contextAccessor, IPdfService pdfService,
             ICommonService commonService,
             IConfiguration configuration)
        {
            _expenseCommandService = expenseCommandService;
            _expenseQueryService = expenseQueryService;
            _contextAccessor = contextAccessor;
            _context = context;
            _pdfService = pdfService;
            _configuration = configuration;
            _commonService = commonService;
        }
        [Authorize(Policy = Permissions.Expenses.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<ExpenseViewModel>, int>>> Search(ExpenseRequestModel request)
        {
            try
            {
                Tuple<List<ExpenseViewModel>, int> content = await _expenseQueryService.SearchAsync(request);
                return await Result<Tuple<List<ExpenseViewModel>, int>>.SuccessAsync(content, "Expense Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<ExpenseViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Expenses.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<ExpenseViewModel>> Detail(string id)
        {
            try
            {
                ExpenseViewModel content = await _expenseQueryService.GetDetailAsync(id);
                return await Result<ExpenseViewModel>.SuccessAsync(content, "Expense Found");
            }
            catch (Exception exception)
            {
                return await Result<ExpenseViewModel>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        // [Authorize(Policy = Permissions.Users.View)]
        //// [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // [Route("reference-no")]
        // [HttpGet]
        // public async Task<IActionResult> GetReferenceNo()
        // {
        //     string referenceNo = await _service.GetReferanceIdAsync();
        //     return Ok(new { ReferenceNo = referenceNo });
        // }
        [Authorize(Policy = Permissions.Expenses.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] ExpenseModel expensemodel)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
            {
                Expense model = new Expense();
                var dbExpenseType =await _context.ExpenseTypes.FindAsync(expensemodel.ExpenseTypeId);
                model.Id = Guid.NewGuid().ToString();
                var count = _context.Expenses.Count() + 1;
                model.ExpenseNo = "E" + count.ToString().PadLeft(7, '0');
                DateTime entryDate;
                var isValidDate = DateTime.TryParseExact(expensemodel.EntryDate, "dd/MM/yyyy", null, DateTimeStyles.None, out entryDate);
                model.BranchId = expensemodel.BranchId;
                model.ExpenseTypeId = expensemodel.ExpenseTypeId;
                model.PaymentMethodId = expensemodel.PaymentMethodId;
                model.Amount = expensemodel.Amount;
                model.Remark = expensemodel.Remark;
                model.CreatedOn = DateTime.Now;
                model.UpdatedOn = DateTime.Now;
                model.EntryDate = isValidDate ? entryDate : DateTime.Now;
                model.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _context.Expenses.AddAsync(model);

                    var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive");
                    if (isAccountsActive)
                    {
                        var paymentMethod = await _context.PaymentMethods.FindAsync(model.PaymentMethodId);
                        if (paymentMethod == null) throw new Exception("Payment Method not Selected");
                       
                            //cash in hand/bank a/c credit
                       await _commonService.HitAccount(_context, model.BranchId, model.ExpenseNo, "Expense",
                              paymentMethod.AccountId, 0, model.Amount,model.EntryDate);
                        
                        //expense a/c debit
                        await _commonService.HitAccount(_context, model.BranchId, model.ExpenseNo, "Expense",
                           dbExpenseType.AccountId, model.Amount, 0,model.EntryDate);
                    }

                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return await Result<string>.SuccessAsync(model.Id, "Expense Added Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return await Result<SupplierPayment>.FailAsync(exception.ToString(), exception.Message);
                }
            }
        }
        [Authorize(Policy = Permissions.Expenses.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/Expense/update")]
        public virtual async Task<Result> Put([FromBody] Expense model)
        {
            try
            {
                model.UpdatedOn = DateTime.UtcNow;
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _expenseCommandService.UpdateAsync(model);
                return await Result<Expense>.SuccessAsync(model, "Expense Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<Expense>.FailAsync(exception.ToString(), exception.Message);
            }

        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(ExpenseRequestModel request)
        {
            try
            {
                request.Page = -1;
                Tuple<List<ExpenseViewModel>, int> content = await _expenseQueryService.SearchAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                   
                    _pdfService.PrintExpenseReportToPdf(stream, _context.Branches.Find(request.BranchId), content.Item1.ToList(), "Expense Report ",request.fromDate,request.ToDate);
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
        [Authorize(Policy = Permissions.Expenses.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                Expense expense = await _expenseCommandService.FindAsync(id);
                if (expense != null)
                {
                    expense.Deleted = true;
                    expense.UpdatedOn = DateTime.UtcNow;
                    expense.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _expenseCommandService.UpdateAsync(expense);
                }
                return await Result<string>.SuccessAsync(id, "Expense Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}
