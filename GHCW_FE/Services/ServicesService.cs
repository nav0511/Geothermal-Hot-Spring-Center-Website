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

        public async Task<ServiceDTO?> GetServiceByID(int id)
        {
            string url = $"Service/{id}";
            return await GetData<ServiceDTO>(url);
        }

        public async Task<HttpStatusCode> UpdateService(ServiceDTO service)
        {
            string url = $"Service/{service.Id}";
            return await PutData(url, service);
        }

        public async Task<HttpStatusCode> DeleteService(int id)
        {
            string url = $"Service/{id}";
            return await DeleteData(url);
        }

        public async Task<HttpStatusCode> CreateService(ServiceDTO service)
        {
            string url = "Service"; 
            return await PushData(url, service); 
        }



    }
}
