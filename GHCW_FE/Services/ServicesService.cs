using GHCW_FE.DTOs;
using System.Net.Http;

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

        public async Task<ServiceDTO> GetServiceById(int id)
        {
            string url = $"Service/GetById/{id}";
            return await GetData<ServiceDTO>(url);
        }
    }
}
