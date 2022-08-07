using Application.Core.Constants;
using Application.Core.Data;
using Application.RequestModel;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
      
        private readonly ICommonService _commonService;
        private readonly IAccountService _accountService;
        public DashboardController(ICommonService commonService, IAccountService accountService)
        {
            _commonService = commonService;
            _accountService = accountService;
           
        }
      

        [HttpPost("sales-statistics")]
        public IActionResult GetSalesStatistics(DashboardRequestModel dashboardRequestModel)
        {
            var fiscalYear = _commonService.GetCurrentFiscalYear();
            var today = DateTime.Today;
            var firstDateOfThisMonth = new DateTime(today.Year, today.Month, 1);
            var firstDateOfLastMonth = firstDateOfThisMonth.AddMonths(-1);
            var lastDateOfLastMonth = firstDateOfThisMonth.AddDays(-1);
            var data = new
            {
                TodaysSale = _commonService.GetSalesTotal(DateTime.Now.Date,DateTime.Now,dashboardRequestModel.BranchId),
                LastSevenDaysSale = _commonService.GetSalesTotal(7,dashboardRequestModel.BranchId),
                ThisMonthsSale = _commonService.GetSalesTotal(firstDateOfThisMonth, DateTime.Now, dashboardRequestModel.BranchId),
                LastMonthSale = _commonService.GetSalesTotal(firstDateOfLastMonth, lastDateOfLastMonth, dashboardRequestModel.BranchId),
                //todays cash info
                TodaysCashInHand = _accountService.CalculateBalanceByAccountId(AccountHeadConstants.CashInHand, AccType.Asset,fiscalYear.Id, fiscalYear.StartDate, DateTime.Now.Date, dashboardRequestModel.BranchId),
                TodaysCashInEximBank = _accountService.CalculateBalanceByAccountId(AccountHeadConstants.CashAtEximBank, AccType.Asset, fiscalYear.Id, fiscalYear.StartDate, DateTime.Now.Date, dashboardRequestModel.BranchId)
            };
        
            return Ok(data);
        }

        #region Utilities
     
        #endregion
    }
}


