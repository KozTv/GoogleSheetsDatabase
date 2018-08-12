using System.Threading.Tasks;

namespace GoogleSheetsDatabase
{
    public interface IGoogleSheetDatabase
    {
        Task DeleteAsync(string table, string uId);
        Task<T> GetAsync<T>(string table, string uId);
        Task SetAsync<T>(string table, string uId, T value);
    }
}
