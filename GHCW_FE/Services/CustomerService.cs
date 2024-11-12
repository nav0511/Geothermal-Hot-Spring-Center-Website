using GHCW_FE.DTOs;
using System.Net;

namespace GHCW_FE.Services
{
    public class CustomerService : BaseService
    {
        public async Task<(HttpStatusCode StatusCode, List<CustomerDTO>? ListCustomer)> ListCustomer(string url, string accessToken)
        {
            var user = await GetData<List<CustomerDTO>>(url, null, accessToken);
            return user;
        }

        public async Task<HttpStatusCode> AddCustomer(string accessToken, AddCustomerRequest ar)
        {
            var statusCode = await PushData<AddCustomerRequest>("Authentication/addcustomer", ar, null, accessToken);
            return statusCode;
        }

        public async Task<HttpStatusCode> EditCustomer(string accessToken, CustomerDTO er)
        {
            var statusCode = await PutData<CustomerDTO>("Authentication/editcustomer", er, null, accessToken);
            return statusCode;
        }

        public async Task<(HttpStatusCode StatusCode, CustomerDTO? CustomerInfo)> GetCustomerById(string accessToken, int uid)
        {
            var customer = await GetData<CustomerDTO>($"Authentication/customer/{uid}", null, accessToken);
            return customer;
        }
    }
}
