using GHCW_FE.DTOs;
using System.Net;

namespace GHCW_FE.Services
{
    public class BillService : BaseService
    {
        public async Task<(HttpStatusCode StatusCode, List<BillDTO>?)> GetBillList(string url, int? role, int? uId)
        {
            if (role.HasValue)
            {
                url += $"?role={role.Value}&uId={uId.Value}";
            }

            return await GetData<List<BillDTO>>(url);
        }

        public async Task<HttpStatusCode> BillActivation(string accessToken, int id)
        {
            var statusCode = await DeleteData($"Bill/BillActivation/{id}", accessToken);
            return statusCode;
        }

        public async Task<(HttpStatusCode StatusCode, List<BillDetailDTO>?)> GetBillDetailsById(int id)
        {
            string url = $"Bill/BillDetail/{id}";
            return await GetData<List<BillDetailDTO>>(url);
        }
        public async Task<HttpStatusCode> SaveBillAsync(BillDTOForBuyProducts bill, string accessToken)
        {
            string url = "Bill/Save";

            return await PushData(url, bill, null, accessToken);
        }
    }
}
