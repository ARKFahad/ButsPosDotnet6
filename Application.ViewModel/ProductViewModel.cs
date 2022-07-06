using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class ProductViewModel
    {
        public ProductViewModel(Product product)
        {
            Id = product.Id;
            Barcode = product.Barcode;
            Name = product.Name;
            PackSize = product.PackSize;
            CategoryId = product.CategoryId;
            GenericNameId = product.GenericNameId;
            SupplierId = product.SupplierId;
            MeasurementUnitId = product.MeasurementUnitId;
            CountryOfOriginId = product.CountryOfOriginId;
            CartonSize = product.CartonSize;
            PurchaseVat = product.PurchaseVat;
            PurchasePrice   = product.PurchasePrice;
            IsVatIncludedInPurchasePrice = product.IsVatIncludedInPurchasePrice;
            Mrp = product.Mrp;
            Category = product.Category != null ? new CategoryViewModel(product.Category) : null;
            Generic = product.GenericName != null ? new GenericNameViewModel(product.GenericName) : null; 
            Supplier = product.Supplier != null ? new SupplierViewModel(product.Supplier) : null;
            MeasurementUnit = product.MeasurementUnit != null ? new MeasurementUnitViewModel(product.MeasurementUnit) : null;
            CountryOfOrigin = product.CountryOfOrigin != null ? new CountryViewModel(product.CountryOfOrigin) : null;
            CreatedOn = product.CreatedOn;
            UpdatedOn = product.UpdatedOn;
            CreatedBy = product.CreatedBy;
            UpdatedBy = product.UpdatedBy;
        }
        public string Id { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string PackSize { get; set; }
        public string CategoryId { get; set; }
        public string GenericNameId { get; set; }
        public string SupplierId { get; set; }
        public string MeasurementUnitId { get; set; }
        public string CountryOfOriginId { get; set; }
        public decimal? CartonSize { get; set; }
        public decimal PurchaseVat { get; set; }
        public bool IsVatIncludedInPurchasePrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal Mrp { get; set; }

        public CategoryViewModel? Category { get; set; }
        public GenericNameViewModel? Generic { get; set; }
        public SupplierViewModel? Supplier { get; set; }
        public MeasurementUnitViewModel? MeasurementUnit { get; set; }
        public CountryViewModel? CountryOfOrigin { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

    }
}
