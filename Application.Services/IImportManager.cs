

namespace Application.Services
{
    /// <summary>
    /// Import manager interface
    /// </summary>
    public partial interface IImportManager
    {
   
        void ImportSupplierFromXlsx(Stream stream);
        void ImportCategoryFromXlsx(Stream stream);
        void ImportGenericFromXlsx(Stream stream);
        void ImportProductFromXlsx(Stream stream);

        void ImportStockItemFromXlsx(Stream stream);

    }
}
