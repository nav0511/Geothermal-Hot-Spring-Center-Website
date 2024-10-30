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
        public async Task UpdateService(Service service)
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteService(Service service)
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }

        public async Task AddService(Service service)
        {

            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
        }

    }
}
