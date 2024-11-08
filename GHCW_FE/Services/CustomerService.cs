using GHCW_FE.DTOs;
using System.Net;

namespace GHCW_FE.Services
{
    public class CustomerService : BaseService
    {
        public async Task<(HttpStatusCode StatusCode, List<CustomerDTO>? ListCustomer)> ListCustomer(string accessToken)
        {
            var user = await GetData<List<CustomerDTO>>("Authentication/CustomerList", null, accessToken);
            return user;
        }
    }
}
