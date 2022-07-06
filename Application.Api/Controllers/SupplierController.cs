using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Enums;
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly IBaseQueryService<Supplier, SupplierRequestModel, SupplierViewModel> _supplierQueryService;
        private readonly IBaseCommandService<Supplier> _supplierCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IPdfService _pdfService;
        private readonly GHP_POS_DBContext _context;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;
        private readonly IImportManager _importManager;
        public SupplierController(IBaseQueryService<Supplier, SupplierRequestModel, SupplierViewModel> supplierQueryService,
            IBaseCommandService<Supplier> supplierCommandService,
            IHttpContextAccessor contextAccessor,
            GHP_POS_DBContext context, 
            IPdfService pdfService,
            ICommonService commonService,
            IConfiguration configuration,
            IImportManager importManager)
        {
            _supplierCommandService = supplierCommandService;
            _supplierQueryService = supplierQueryService;
            _contextAccessor = contextAccessor;
            _context = context;
            _pdfService = pdfService;
            _commonService = commonService;
            _configuration = configuration;
            _importManager = importManager;
        }
        [Authorize(Policy = Permissions.Suppliers.ViewAll)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<SupplierViewModel>, int>>> Search(SupplierRequestModel request)
        {
            try
            {
               
                Tuple<List<SupplierViewModel>, int> content = await _supplierQueryService.SearchAsync(request);
                return await Result<Tuple<List<SupplierViewModel>, int>>.SuccessAsync(content, "Suppliers Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<SupplierViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(SupplierRequestModel request)
        {
            try
            {
                request.Page = -1;
                
                request.IsAscending = true;
                Tuple<List<SupplierViewModel>, int> content = await _supplierQueryService.SearchAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintSupplierBalanceReportToPdf(stream,content.Item1, "Supplier Balance Report ");
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
        [Authorize(Policy = Permissions.Suppliers.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<SupplierViewModel>> Detail(string id)
        {
            try
            {
                SupplierViewModel content = await _supplierQueryService.GetDetailAsync(id);
                content.IsAccountActive= _configuration.GetValue<bool>("AccountSettings:IsActive");
                return await Result<SupplierViewModel>.SuccessAsync(content, "Supplier Found");
            }
            catch (Exception exception)
            {
                return await Result<SupplierViewModel>.FailAsync(exception.ToString(), exception.Message);
            }
        }

       
        [Route("code")]
        [HttpGet]
        public IActionResult GetReferenceNo()
        {
            string code =  _supplierCommandService.GetReferanceId("S");
            return Ok(new { Code = code });
        }
        [Authorize(Policy = Permissions.Suppliers.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] Supplier model)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {   //save Account Head
                    var accountHead = new AccountHead
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = _commonService.GetAccountCode(AccType.Liability),
                        AccountTypeId = AccType.Liability,//liablility account id=2
                        Level = await _commonService.GetAccountLevel("15"),
                        Name = model.Name,
                        ParentId = "15",//parent Sundry Creditors from database "15"
                        IsControlAccount = true,
                        CreatedOn = DateTime.Now,
                        CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId(),
                        UpdatedOn = DateTime.Now,
                        UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId()
                    };
                    await _context.AccountHeads.AddAsync(accountHead);
                    await _context.SaveChangesAsync();
                    //save supplier
                    var count = _context.Suppliers.Count() + 1;
                    model.Id = Guid.NewGuid().ToString();
                    model.Code = "S" + count.ToString().PadLeft(7, '0');
                    model.Balance = model.OpeningBalance;
                    model.AccountId = accountHead.Id;
                    model.CreatedOn = DateTime.Now;
                    model.UpdatedOn = DateTime.Now;
                    model.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _context.Suppliers.AddAsync(model);

                  

                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                    return await Result<string>.SuccessAsync(model.Id, "Supplier Added Successfully");
                }
                catch (Exception exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    return await Result<string>.FailAsync(exception.ToString(), exception.Message);
                }
            }
        }
        [Authorize(Policy = Permissions.Suppliers.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/supplier/update")]
        public virtual async Task<Result> Put([FromBody] Supplier model)
        {
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    //update Account Head
                    var accountHead = await _context.AccountHeads.FindAsync(model.AccountId);
                    if(accountHead != null)
                    {
                        accountHead.Name = model.Name;
                        _context.AccountHeads.Update(accountHead);
                        await _context.SaveChangesAsync();
                    }
                   
                    //update supplier
                    model.UpdatedOn = DateTime.Now;
                    model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    _context.Suppliers.Update(model);
                    await _context.SaveChangesAsync();

                    await dbContextTransaction.CommitAsync();
                return await Result<string>.SuccessAsync(model.Id, "Supplier Updated Successfully");
             }
            catch (Exception exception)
            {
                await dbContextTransaction.RollbackAsync();
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }

    }
        [Authorize(Policy = Permissions.Suppliers.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                Supplier supplier = await _supplierCommandService.FindAsync(id);
                if (supplier != null)
                {
                    supplier.Deleted = true;
                    supplier.UpdatedOn = DateTime.Now;
                    supplier.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _supplierCommandService.UpdateAsync(supplier);
                }
               
                return await Result<string>.SuccessAsync(id, "Supplier Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        [HttpPost]
        [Route("upload")]
        public virtual async Task<Result> ImportFromXlsx(IFormFile importexcelfile)
        {
           
            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    _importManager.ImportStockItemFromXlsx(importexcelfile.OpenReadStream());
                }
                else
                {
                    throw new Exception("Upload error");
                }
                return await Result<string>.SuccessAsync("", "Supplier Uploaded Successfully");

            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}
