using GHCW_FE.DTOs;
using System.Net;

namespace GHCW_FE.Services
{
    public class ServicesService : BaseService
    {
        public async Task<(HttpStatusCode StatusCode, List<ServiceDTO>?)> GetServices(string url)
        {
            return await GetData<List<ServiceDTO>>(url);
        }
        public async Task<(HttpStatusCode StatusCode, int Total)> GetTotalServices()
        {
            string url = "Service/Total";
            return await GetData<int>(url);
        }

        public async Task<(HttpStatusCode StatusCode, ServiceDTO?)> GetServiceById(int id)
        {
            string url = $"Service/{id}";
            return await GetData<ServiceDTO>(url);
        }

        public async Task<HttpStatusCode> UpdateService(ServiceDTOForUpdate service, string accessToken, string? accepttype = null)
        {
            string url = $"Service/{service.Id}";
            return await PutData(url, service, accepttype, accessToken);
        }

        public async Task<HttpStatusCode> DeleteService(int id)
        {
            string url = $"Service/{id}";
            return await DeleteData(url);
        }

        public async Task<HttpStatusCode> CreateService(ServiceDTO2 service, string accessToken, string? accepttype = null)
        {
            string url = "Service";
            if (accepttype != null) return await PushData(url, service, accepttype, accessToken);
            else return await PushData(url, service, null, accessToken);
        }

        public async Task<HttpStatusCode> ServiceActivation(string accessToken, int id)
        {
            var statusCode = await DeleteData($"Service/ServiceActivation/{id}", accessToken);
            return statusCode;
        }

    }
}