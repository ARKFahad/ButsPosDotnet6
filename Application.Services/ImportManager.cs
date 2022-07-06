using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.PermissionHelpers;
using Application.Services;
using Application.Services.ExcelHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;

namespace Application.Services
{
    /// <summary>
    /// Import manager
    /// </summary>
    public  class ImportManager : IImportManager
    {
        private readonly GHP_POS_DBContext _context;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        public ImportManager(GHP_POS_DBContext context,
            ICommonService commonService,
            IConfiguration configuration,
            IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _commonService = commonService;
            _configuration = configuration;
            _contextAccessor = contextAccessor;
        }
        public static IList<PropertyByName<T>> GetPropertiesByExcelCells<T>(ExcelWorksheet worksheet)
        {
            var properties = new List<PropertyByName<T>>();
            var poz = 1;
            while (true)
            {
                try
                {
                    var cell = worksheet.Cells[1, poz];

                    if (string.IsNullOrEmpty(cell?.Value?.ToString()))
                        break;

                    poz += 1;
                    properties.Add(new PropertyByName<T>(cell.Value.ToString()));
                }
                catch
                {
                    break;
                }
            }

            return properties;
        }




        public virtual  void ImportSupplierFromXlsx(Stream stream)
        {
            using var xlPackage = new ExcelPackage(stream);
            // get the first worksheet in the workbook
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("No worksheet found");

            //the columns
            var properties = GetPropertiesByExcelCells<Supplier>(worksheet);

            var manager = new PropertyManager<Supplier>(properties);

            var iRow = 2;

            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                    break;
                manager.ReadFromXlsx(worksheet, iRow);
                var supplier = _context.Suppliers.Find(manager.GetProperty("Id").StringValue);
                var isNew = supplier == null;
                supplier ??= new Supplier();
                var accountHead = new AccountHead();
                if (isNew)
                {
                    accountHead.Id = Guid.NewGuid().ToString();
                    accountHead.Code = _commonService.GetAccountCode(AccType.Liability);
                    accountHead.AccountTypeId = AccType.Liability;//liablility account id=2
                    accountHead.Level = 5;
                   // accountHead.Name = supplier.Name;
                    accountHead.ParentId = "15";//parent Sundry Creditors from database "15"
                    accountHead.IsControlAccount = true;
                    accountHead.CreatedOn = DateTime.Now;
                    accountHead.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    accountHead.UpdatedOn = DateTime.Now;
                    accountHead.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    //save supplier
                    var count = _context.Suppliers.Count() + 1;
                    supplier.Id = Guid.NewGuid().ToString();
                    supplier.Code = "S" + count.ToString().PadLeft(7, '0');
                    supplier.AccountId = accountHead.Id;
                    supplier.CreatedOn = DateTime.Now;
                    supplier.UpdatedOn = DateTime.Now;
                    supplier.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    supplier.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();

                }
                foreach (var property in manager.GetProperties)
                {
                    switch (property.PropertyName)
                    {
                       
                        case "Name":
                            supplier.Name = property.StringValue;
                            accountHead.Name = property.StringValue;
                            break;
                    }
                }
                //update supplier
                supplier.UpdatedOn = DateTime.Now;
                supplier.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();

                if (isNew)
                {
                    using (var dbContextTransaction =  _context.Database.BeginTransaction())
                    {
                        try
                       {
                            //save Account Head
                             _context.AccountHeads.Add(accountHead);
                             _context.Suppliers.Add(supplier);
                             _context.SaveChanges();
                             dbContextTransaction.Commit();

                        }
                        catch (Exception exception)
                        {
                             dbContextTransaction.Rollback();

                        }
                    }
                }
                else
                {
                    _context.Suppliers.Update(supplier);
                     _context.SaveChanges();
                }
                iRow++;
            }

          
        }


        public virtual void ImportCategoryFromXlsx(Stream stream)
        {
            using var xlPackage = new ExcelPackage(stream);
            // get the first worksheet in the workbook
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("No worksheet found");

            //the columns
            var properties = GetPropertiesByExcelCells<Category>(worksheet);

            var manager = new PropertyManager<Category>(properties);

            var iRow = 2;

            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                    break;

                manager.ReadFromXlsx(worksheet, iRow);
                var category = _context.Categories.Find(manager.GetProperty("Id").StringValue);
                var isNew = category == null;
                category ??= new Category();
              
                if (isNew)
                {
                    category.Id = Guid.NewGuid().ToString();
                    category.CreatedOn = DateTime.Now;
                    category.UpdatedOn = DateTime.Now;
                    category.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    category.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                }

                foreach (var property in manager.GetProperties)
                {
                    switch (property.PropertyName)
                    {

                        case "Name":
                            category.Name = property.StringValue;
                            break;
                    }
                }
                category.UpdatedOn = DateTime.Now;
                category.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();

                if (isNew)
                {
                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            _context.Categories.Add(category);
                            _context.SaveChanges();
                            dbContextTransaction.Commit();

                        }
                        catch (Exception exception)
                        {
                            dbContextTransaction.Rollback();

                        }
                    }
                }
                else
                {
                    _context.Categories.Update(category);
                    _context.SaveChanges();
                }
                iRow++;
            }


        }



        public virtual void ImportGenericFromXlsx(Stream stream)
        {
            using var xlPackage = new ExcelPackage(stream);
            // get the first worksheet in the workbook
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("No worksheet found");

            //the columns
            var properties = GetPropertiesByExcelCells<GenericName>(worksheet);

            var manager = new PropertyManager<GenericName>(properties);

            var iRow = 2;

            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                    break;

                manager.ReadFromXlsx(worksheet, iRow);
                var genericName = _context.GenericNames.Find(manager.GetProperty("Id").StringValue);
                var isNew = genericName == null;
                genericName ??= new GenericName();

                if (isNew)
                {
                    genericName.Id = Guid.NewGuid().ToString();
                    genericName.CreatedOn = DateTime.Now;
                    genericName.UpdatedOn = DateTime.Now;
                    genericName.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    genericName.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();

                }

                foreach (var property in manager.GetProperties)
                {
                    switch (property.PropertyName)
                    {
                        case "Name":
                            genericName.Name = property.StringValue;
                            break;
                        case "CategoryId":
                            genericName.CategoryId =_context.Categories.Where(x=>x.Name== property.StringValue).FirstOrDefault().Id;
                            break;
                    }
                }
                genericName.UpdatedOn = DateTime.Now;
                genericName.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                if (isNew)
                {
                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            _context.GenericNames.Add(genericName);
                            _context.SaveChanges();
                            dbContextTransaction.Commit();
                        }
                        catch (Exception exception)
                        {
                            dbContextTransaction.Rollback();

                        }
                    }
                }
                else
                {
                    _context.GenericNames.Update(genericName);
                    _context.SaveChanges();
                }
                iRow++;
            }
        }


        public virtual void ImportProductFromXlsx(Stream stream)
        {
            using var xlPackage = new ExcelPackage(stream);
            // get the first worksheet in the workbook
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("No worksheet found");

            //the columns
            var properties = GetPropertiesByExcelCells<Product>(worksheet);

            var manager = new PropertyManager<Product>(properties);
            var iRow = 2;
            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                    break;

                manager.ReadFromXlsx(worksheet, iRow);
                var product = _context.Products.Find(manager.GetProperty("Id").StringValue);
                var isNew = product == null;
                product ??= new Product();
                if (isNew)
                {
                    var count = _context.Products.Count() + 1;
                    product.Id = Guid.NewGuid().ToString();
                    product.Barcode = "P" + count.ToString().PadLeft(7, '0');
                    product.MeasurementUnitId = "f87543f6-e24d-421a-b5ab-70fbed465f03";
                    product.CountryOfOriginId = "957e16dd-66c9-48d6-9275-86e0115d734d";
                    product.CreatedOn = DateTime.Now;
                    product.UpdatedOn = DateTime.Now;
                    product.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    product.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                }
                foreach (var property in manager.GetProperties)
                {
                    switch (property.PropertyName)
                    {

                        case "TempCode":
                            product.TempCode = property.StringValue;
                            break;
                        case "Name":
                            product.Name = property.StringValue;
                            break;
                        case "CategoryId":
                            product.CategoryId = _context.Categories.Where(x => x.Name == property.StringValue).FirstOrDefault().Id;
                            break;
                        case "GenericNameId":
                            product.GenericNameId = _context.GenericNames.Where(x => x.Name == property.StringValue).FirstOrDefault().Id;
                            break;
                        case "SupplierId":
                            product.SupplierId = _context.Suppliers.Where(x => x.Name == property.StringValue).FirstOrDefault().Id;
                            break;
                        case "PurchasePrice":
                            product.PurchasePrice = property.DecimalValue;
                            break;
                        case "PurchaseVat":
                            product.PurchaseVat = property.DecimalValue;
                            break;
                        case "MRP":
                            product.Mrp = property.DecimalValue;
                            break;
                        case "PackSize":
                            product.PackSize = property.StringValue;
                            break;
                        case "CartonSize":
                            product.CartonSize = property.DecimalValue;
                            break;
                    }
                }
                //update supplier
                product.UpdatedOn = DateTime.Now;
                product.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                if (isNew)
                {
                _context.Products.Add(product);
                                         
                }

                else
                {
                    _context.Products.Update(product);
                    _context.SaveChanges();
                }
                iRow++;
            }
            _context.SaveChanges();
        }

        public virtual void ImportStockItemFromXlsx(Stream stream)
        {
            var userId = _contextAccessor.HttpContext?.User?.GetUserId();
            using var xlPackage = new ExcelPackage(stream);
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("No worksheet found");
            var properties = GetPropertiesByExcelCells<Product>(worksheet);
            var manager = new PropertyManager<Product>(properties);
            var iRow = 2;
            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));
                if (allColumnsAreEmpty)
                    break;
                manager.ReadFromXlsx(worksheet, iRow);
                var qty = manager.GetProperty("Quantity").IntValue;
                var product = _context.Products.Where(x => x.TempCode == manager.GetProperty("TempCode").StringValue).FirstOrDefault();
                if (qty> 0)
                {
                    for (int i = 0; i < qty; i++)
                    {
                        _context.StockItems.Add(new StockItem
                        {
                            Id=Guid.NewGuid().ToString(),
                            Quantity=1,
                            CreatedOn=DateTime.Now,
                            CreatedBy=userId,
                            BranchId= manager.GetProperty("BranchId").StringValue,
                            PurchaseRate = manager.GetProperty("PurchaseRate").DecimalValue,
                            ExpiryDate = manager.GetProperty("ExpiryDate").DateTimeNullable,
                            SupplierId=product?.SupplierId,
                            ProductId=product?.Id
                        });
                      
                    }                    
                }
                 iRow++;
            }
            _context.SaveChanges();
        }
    }
}