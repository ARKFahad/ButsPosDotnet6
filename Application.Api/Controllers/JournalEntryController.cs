using Application.Core.Common;
using Application.Core.Constants;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JournalEntryController : ControllerBase
    {
        private readonly GHP_POS_DBContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IPdfService _pdfService;
        private readonly IBaseQueryService<JournalEntry, JournalEntryRequestModel, JournalEntryViewModel> _journalEntryQueryService;
        private readonly IConfiguration _configuration;
        private readonly ICommonService _commonService;

        public JournalEntryController(IBaseQueryService<JournalEntry, JournalEntryRequestModel, JournalEntryViewModel> journalEntryQueryService, 
            GHP_POS_DBContext context, IHttpContextAccessor contextAccessor, IPdfService pdfService,
            IConfiguration configuration, ICommonService commonService
            )
        {
            _journalEntryQueryService = journalEntryQueryService;
            _context = context;
            _contextAccessor = contextAccessor;
            _pdfService = pdfService;
            _commonService = commonService;
            _configuration = configuration;
        }
        [Authorize(Policy = Permissions.JournalEntries.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]

        public virtual async Task<Result<Tuple<List<JournalEntryViewModel>, int>>> Search(JournalEntryRequestModel request)
        {
            try
            {
                request.FiscalYearId = _context.FiscalYears.Where(x => x.IsActive).FirstOrDefault()?.Id;
                Tuple<List<JournalEntryViewModel>, int> content = await _journalEntryQueryService.SearchAsync(request);
                return await Result<Tuple<List<JournalEntryViewModel>, int>>.SuccessAsync(content, "JournalEntrys Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<JournalEntryViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
     

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Policy = Permissions.JournalEntries.Add)]
        public async Task<IActionResult> AddJournalEntryAsync([FromBody] JournalEntryAddRequestModel model)
           {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {

                    DateTime transactionDate;
                    var isValidDate = DateTime.TryParseExact(model.TransactionDate, "dd/MM/yyyy", null, DateTimeStyles.None, out transactionDate);
                    //journalEntry add
                    JournalEntry journalEntry = new JournalEntry();
                    var count = _context.JournalEntries.Count() + 1;
                    
                        journalEntry.Id = Guid.NewGuid().ToString();
                        journalEntry.Vnumber = "JV" + count.ToString().PadLeft(7, '0');
                        journalEntry.FiscalYearId = _context.FiscalYears.Where(x => x.IsActive).FirstOrDefault()?.Id;
                        journalEntry.BranchId = model.BranchId;
                        journalEntry.Description = model.Description;
                        journalEntry.TransactionDate = transactionDate;
                        journalEntry.CreatedOn = DateTime.Now;
                        journalEntry.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                        await _context.JournalEntries.AddAsync(journalEntry);


                    //journalEntry items add
                    foreach (var item in model.JournalEntryLines)
                    {
                        var journalEntryLine = new JournalEntryLine
                        {
                            Id = Guid.NewGuid().ToString(),
                            JournalEntryId = journalEntry.Id,
                            AccountId = item.AccountId,
                            IsOpeningBalance = item.IsOpeningBalance
                        };
                        if (item.PostType == "DR") journalEntryLine.Debit = item.Amount;
                        else journalEntryLine.Credit = item.Amount;
                        _context.JournalEntryLines.Add(journalEntryLine);

                        //account entry
                        var isAccountsActive = _configuration.GetValue<bool>("AccountSettings:IsActive"); 
                        if (isAccountsActive)
                        {
                            await _commonService.HitAccount(_context, journalEntry.BranchId, journalEntry.Vnumber, "Journal Entry",
                               journalEntryLine.AccountId, journalEntryLine.Debit, journalEntryLine.Credit,journalEntry.TransactionDate, journalEntryLine.IsOpeningBalance);
                        }
                    }
                        await _context.SaveChangesAsync();
                        await dbContextTransaction.CommitAsync();


                    return Ok(new { JournalEntryId = journalEntry.Id,Succeeded=true,Message="JournalEntry Added Successfully" });
                }
                catch (Exception e)
                {
                    await dbContextTransaction.RollbackAsync();
                    return Ok(new { JournalEntryId = "",Succeeded=false, Message = e.Message });
                }
            }
        }

        //   [Authorize(Policy = Permissions.JournalEntries.View)]
        //  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("voucher-no")]
        [HttpGet]
        public IActionResult GetVoucherNo()
        {
            var count = _context.JournalEntries.Count() + 1;
            return Ok(new {VoucherNo= "JV" + count.ToString().PadLeft(7, '0') });
         
        }
       


    }
}



