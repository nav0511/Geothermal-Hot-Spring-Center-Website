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
            var statusCode = await PushData<AddCustomerRequest>("Customer/addcustomer", ar, null, accessToken);
            return statusCode;
        }

        public async Task<HttpStatusCode> EditCustomer(string accessToken, CustomerDTO er)
        {
            var statusCode = await PutData<CustomerDTO>("Customer/editcustomer", er, null, accessToken);
            return statusCode;
        }

        public async Task<(HttpStatusCode StatusCode, CustomerDTO? CustomerInfo)> GetCustomerById(string accessToken, int uid)
        {
            var customer = await GetData<CustomerDTO>($"Customer/customer/{uid}", null, accessToken);
            return customer;
        }

        public async Task<HttpStatusCode> Subscriber(Subscriber s)
        {
            var statusCode = await PutData("Customer/editsubscribe", s);
            return statusCode;
        }

        public async Task<(HttpStatusCode StatusCode, CustomerDTO? CustomerInfo)> GetCustomerByEmail(string accessToken, string email)
        {
            var customer = await GetData<CustomerDTO>($"Customer/customer/getByEmail/{email}", null, accessToken);
            return customer;
        }
    }
}
