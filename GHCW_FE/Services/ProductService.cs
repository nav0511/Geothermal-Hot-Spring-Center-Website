using GHCW_FE.DTOs;
using System.Net;

namespace GHCW_FE.Services
{
    public class ProductService : BaseService
    {
        public async Task<List<ProductDTO>?> GetProducts(string url)
        {
            return await GetData<List<ProductDTO>>(url);
        }
        public async Task<int> GetTotalProducts()
        {
            string url = "Product/Total";
            return await GetData<int>(url);
        }

        public async Task<ProductDTO?> GetProductByID(int id)
        {
            string url = $"Product/{id}";
            return await GetData<ProductDTO>(url);
        }

        public async Task<HttpStatusCode> UpdateProduct(ProductDTO product)
        {
            string url = $"Product/{product.Id}";
            return await PutData(url, product);
        }

        public async Task<HttpStatusCode> DeleteProduct(int id)
        {
            string url = $"Product/{id}";
            return await DeleteData(url);
        }

        public async Task<HttpStatusCode> CreateProduct(ProductDTOImg product, string? accepttype = null)
        {
            string url = "Product";
            if (accepttype != null) return await PushData(url, product, accepttype);
            else return await PushData(url, product);
        }
    }
}
