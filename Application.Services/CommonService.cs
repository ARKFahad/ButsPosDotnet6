using Application.Core.Common;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.PermissionHelpers;
using Application.Repositories;
using Application.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
   public class CommonService:ICommonService
    {
        private readonly IBaseRepository<Product> _productRepository;
        private readonly IBaseRepository<Sale> _saleRepository;
        private readonly IBaseRepository<FiscalYear> _fiscalYearRepository;
        private readonly IBaseRepository<Category> _categoryRepository;
        private readonly IBaseRepository<AccountHead> _accountHeadRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        public CommonService(IBaseRepository<Product> productRepository, IBaseRepository<Category> categoryRepository, 
            IBaseRepository<Sale> saleRepository, IBaseRepository<AccountHead> accountHeadRepository,
            IBaseRepository<FiscalYear> fiscalYearRepository,
           IHttpContextAccessor contextAccessor)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _saleRepository = saleRepository;
            _accountHeadRepository = accountHeadRepository;
            _fiscalYearRepository = fiscalYearRepository;
            _contextAccessor = contextAccessor;
        }

        public  async Task<List<ProductAutocompleteViewModel>>  SearchProductAutocomplete(string supplierid)
        {
           
            var queryable = _productRepository.GetAllAsNoTracking().Include(x=>x.Category).Where(x=>x.SupplierId==supplierid).Where(x=>!x.Deleted);
           
            var list= await queryable.Select(x => new ProductAutocompleteViewModel
            {
                Id=x.Id,
                Barcode=x.Barcode,
                ProductName=x.Name+" "+ x.PackSize+" "+x.Category.Name,
                PurchaseVat=x.PurchaseVat,
                PurchasePrice=x.PurchasePrice,
                IsVatIncludedInPurchasePrice=x.IsVatIncludedInPurchasePrice,
                Mrp=x.Mrp
            }).ToListAsync();
            return list;
        }

        public decimal GetSalesTotal(int days, string? branchId = "")
        {
            DateTime date = DateTime.Now.AddDays(-days);
            var sales = _saleRepository.GetAll().Where(x => !x.Deleted);
            if (!string.IsNullOrWhiteSpace(branchId)) sales = sales.Where(x => x.BranchId == branchId);
            return sales.Where(x => x.CreatedOn >= date).Sum(x => x.PayableAmount);
        }


        public decimal GetSalesTotal(DateTime? createdFrom = null, DateTime? createdTo = null, string? branchId = "")
        {
            var saleCreatedFrom = createdFrom.HasValue ? createdFrom.Value : new DateTime(1900, 1, 1);
            var saleCreatedTo = createdTo.HasValue ? createdTo.Value.AddDays(1) : saleCreatedFrom.AddDays(1);
            var sales = _saleRepository.GetAll().Where(x => !x.Deleted);
            if (!string.IsNullOrWhiteSpace(branchId)) sales = sales.Where(x => x.BranchId == branchId);
            return sales.Where(x => saleCreatedFrom <= x.CreatedOn && saleCreatedTo >= x.CreatedOn).Sum(x => x.PayableAmount);

        }
        public decimal GetCashPaidSalesTotal(DateTime? createdFrom = null, DateTime? createdTo = null, string? branchId = "")
        {
            var saleCreatedFrom = createdFrom.HasValue ? createdFrom.Value : new DateTime(1900, 1, 1);
            var saleCreatedTo = createdTo.HasValue ? createdTo.Value : saleCreatedFrom.AddDays(1);
            var sales = _saleRepository.GetAll().Where(x => !x.Deleted);
            if (!string.IsNullOrWhiteSpace(branchId)) sales = sales.Where(x => x.BranchId == branchId);
            return sales.Where(x => saleCreatedFrom <= x.CreatedOn && saleCreatedTo >= x.CreatedOn).Sum(x => x.PaidAmount);

        }

        public decimal GetCreditSalesTotal(DateTime? createdFrom = null, DateTime? createdTo = null, string? branchId = "")
        {
            var saleCreatedFrom = createdFrom.HasValue ? createdFrom.Value : new DateTime(1900, 1, 1);
            var saleCreatedTo = createdTo.HasValue ? createdTo.Value : saleCreatedFrom.AddDays(1);
            var sales = _saleRepository.GetAll().Where(x => !x.Deleted);
            if (!string.IsNullOrWhiteSpace(branchId)) sales = sales.Where(x => x.BranchId == branchId);
            return sales.Where(x => saleCreatedFrom <= x.CreatedOn && saleCreatedTo >= x.CreatedOn).Sum(x => x.DueAmount);

        }
        public decimal GetCustomerDueCollectionTotal(DateTime? createdFrom = null, DateTime? createdTo = null, string? branchId = "")
        {
            var saleCreatedFrom = createdFrom.HasValue ? createdFrom.Value : new DateTime(1900, 1, 1);
            var saleCreatedTo = createdTo.HasValue ? createdTo.Value : saleCreatedFrom.AddDays(1);
            var sales = _saleRepository.GetAll().Where(x => !x.Deleted);
            if (!string.IsNullOrWhiteSpace(branchId)) sales = sales.Where(x => x.BranchId == branchId);
            return sales.Where(x => saleCreatedFrom <= x.CreatedOn && saleCreatedTo >= x.CreatedOn).Sum(x => x.DueAmount);

        }
        public string GetAccountCode(string accountTypeId)
        {
            var count = _accountHeadRepository.GetAll().Where(x => x.AccountTypeId == accountTypeId).Count() + 1;
            string code = "";
            switch (accountTypeId)
            {
                case "1":
                    code = "1" + count.ToString().PadLeft(4, '0');
                    break;
                case "2":
                    code = "2" + count.ToString().PadLeft(4, '0');
                    break;
                case "3":
                    code = "3" + count.ToString().PadLeft(4, '0');
                    break;
                case "4":
                    code = "4" + count.ToString().PadLeft(4, '0');
                    break;
                default:
                    code = "5" + count.ToString().PadLeft(4, '0');
                    break;
            }
            return code;
        }
        public async Task<int> GetAccountLevel(string parentId)
        {
            var parentAccount =await _accountHeadRepository.FindAsync(parentId);
            return parentAccount.Level + 1;
        }
        
        public async Task HitAccount(GHP_POS_DBContext context,string branchId, string vnumber, string description, string accountId,decimal debit, decimal credit,DateTime transactionDate,bool isOpeningBalance=false)
        {

            var accountEntry = new AccountEntry
            {
                Id = Guid.NewGuid().ToString(),
                FiscalYearId = context.FiscalYears.Where(x => x.IsActive)?.FirstOrDefault()?.Id,
                BranchId = branchId,
                Vnumber = vnumber,
                Description = description,
                AccountId = accountId,
                Debit = debit,
                Credit = credit,
                TransactionDate = transactionDate,
                IsOpeningBalance = isOpeningBalance,
                CreatedBy = _contextAccessor.HttpContext.User?.GetUserId()
            };
            await context.AccountEntries.AddAsync(accountEntry);
        }
    }
}
