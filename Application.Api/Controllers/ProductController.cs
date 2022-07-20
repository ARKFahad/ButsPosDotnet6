using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.PermissionHelpers;
using Application.RequestModel;
using Application.ViewModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PbApp.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IBaseQueryService<Product, ProductRequestModel, ProductViewModel> _productQueryService;
        private readonly IBaseCommandService<Product> _productCommandService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly GHP_POS_DBContext _context;
        public ProductController(IBaseQueryService<Product, ProductRequestModel, ProductViewModel> productQueryService,
            IBaseCommandService<Product> productCommandService,
            IHttpContextAccessor contextAccessor,
            GHP_POS_DBContext context)
        {
            _productCommandService = productCommandService;
            _productQueryService = productQueryService;
            _contextAccessor = contextAccessor;
            _context = context;
        }
        [AllowAnonymous]
        [Authorize(Policy = Permissions.Products.ViewAll)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<ProductViewModel>, int>>> Search(ProductRequestModel request)
        {
            try
            {
                Tuple<List<ProductViewModel>, int> content = await _productQueryService.SearchAsync(request);
                return await Result<Tuple<List<ProductViewModel>, int>>.SuccessAsync(content, "Products Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<ProductViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Products.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<ProductViewModel>> Detail(string id)
        {
            try
            {
                ProductViewModel content = await _productQueryService.GetDetailAsync(x=>x.Id==id,"Category;GenericName;Supplier");
                return await Result<ProductViewModel>.SuccessAsync(content, "Product Found");
            }
            catch (Exception exception)
            {
                return await Result<ProductViewModel>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        [Authorize(Policy = Permissions.Products.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("barcode")]
        [HttpGet]
        public IActionResult GetReferenceNo()
        {
            string barcode = _productCommandService.GetReferanceId(character:"A");
            return Ok(new { Barcode = barcode });
        }
        [Authorize(Policy = Permissions.Products.Add)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] Product model)
        {
            try
            {
                model.TempCode= _productCommandService.GetReferanceId(character: "A");
                model.Id = Guid.NewGuid().ToString();
                model.CreatedOn = DateTime.Now;
                model.UpdatedOn = DateTime.Now;
                model.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                model.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _context.Products.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<string>.SuccessAsync(model.Id, "Product Added Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Products.Update)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/product/update")]
        public virtual async Task<Result> Put([FromBody] Product model)
        {
            try
            {
                Product dbProduct =await _context.Products.FindAsync(model.Id);
                if (dbProduct == null) throw new NotFoundResultException("Not Found");
                dbProduct.Barcode = model.Barcode;
                dbProduct.Name = model.Name;
                dbProduct.PackSize = model.PackSize;
                dbProduct.CategoryId = model.CategoryId;
                dbProduct.GenericNameId=model.GenericNameId;
                dbProduct.SupplierId = model.SupplierId;
                dbProduct.MeasurementUnitId = model.MeasurementUnitId;
                dbProduct.CountryOfOriginId = model.CountryOfOriginId;
                dbProduct.CartonSize = model.CartonSize;
                dbProduct.PurchaseVat=model.PurchaseVat;
                dbProduct.PurchasePrice = model.PurchasePrice;
                dbProduct.Mrp = model.Mrp;
                dbProduct.IsExpiryItem = model.IsExpiryItem;
                dbProduct.IsVatIncludedInPurchasePrice=model.IsVatIncludedInPurchasePrice;
                dbProduct.TempCode = model.TempCode;
                dbProduct.UpdatedOn = DateTime.Now;
                dbProduct.UpdatedBy= _contextAccessor.HttpContext?.User?.GetUserId();
                 _context.Products.Update(dbProduct);
                await _context.SaveChangesAsync();
                return await Result<string>.SuccessAsync(model.Id, "Product Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }

        }
        [Authorize(Policy = Permissions.Products.Remove)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                Product product = await _productCommandService.FindAsync(id);
                if (product != null)
                {
                    product.Deleted = true;
                    product.UpdatedOn = DateTime.Now;
                    product.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _productCommandService.UpdateAsync(product);
                }
                return await Result<string>.SuccessAsync(id, "Product Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
    }
}
