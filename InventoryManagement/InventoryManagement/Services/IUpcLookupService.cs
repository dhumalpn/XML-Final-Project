using InventoryManagement.Models;

namespace InventoryManagement.Services
{
    public interface IUpcLookupService
    {
        Task<Item?> LookupUpcAsync(string upc);
    }
}
