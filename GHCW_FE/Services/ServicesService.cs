using GHCW_FE.DTOs;

namespace GHCW_FE.Services
{
    public class ServicesService : BaseService
    {
        public async Task<List<ServiceDTO>?> GetServices(string url)
        {
            return await GetData<List<ServiceDTO>>(url);
        }
        public async Task<int> GetTotalServices()
        {
            string url = "Service/Total";
            return await GetData<int>(url);
        }
    }
}
