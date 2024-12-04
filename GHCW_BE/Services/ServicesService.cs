using GHCW_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Services
{
    public class ServicesService
    {
        private readonly GHCWContext _context;

        public ServicesService(GHCWContext context)
        {
            _context = context;
        }

        public IQueryable<Service> GetListServices()
        {
            return _context.Services.AsQueryable();
        }

        public async Task<Service> GetServiceById(int id)
        {
            return await _context.Services.FirstOrDefaultAsync(n => n.Id == id);
        }
        
        public async Task<(bool isSuccess, string message)> UpdateService(Service service)
        {
            try
            {
                _context.Services.Update(service);
                await _context.SaveChangesAsync();
                return (true, "Cập nhật Dịch vụ mới thành công.");

            }
            catch (Exception)
            {
                return (false, "Có lỗi trong quá trình cập nhật Dịch vụ, vui lòng thử lại.");
            }


        }

        public async Task DeleteService(Service service)
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }

        public async Task<(bool isSuccess, string message)> AddService(Service service)
        {
            try
            {
                await _context.Services.AddAsync(service);
                await _context.SaveChangesAsync();
                return (true, "Thêm Dịch vụ mới thành công.");
            }
            catch (Exception)
            {
                return (false, "Có lỗi trong quá trình thêm Dịch vụ, vui lòng thử lại.");
            }

        }

        public async Task<(bool isSuccess, string message)> ServiceActivation(int sid)
        {
            var service = await _context.Services.FindAsync(sid);
            if (service == null)
            {
                return (false, "Dịch vụ không tồn tại.");
            }
            try
            {
                service.IsActive = !service.IsActive;
                _context.Services.Update(service);
                await _context.SaveChangesAsync();

                return (true, "Thay đổi trạng thái dịch vụ thành công.");
            }
            catch (Exception)
            {
                return (false, "Thay đổi trạng thái dịch vụ thất bại, vui lòng thử lại.");
            }
        }

    }
}
