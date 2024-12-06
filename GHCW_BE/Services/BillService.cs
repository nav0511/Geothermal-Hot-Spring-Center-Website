using AutoMapper;
using GHCW_BE.Models;
using GHCW_BE.Utils.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Services
{
    public class BillService
    {
        private readonly GHCWContext _context;
        private readonly IConfiguration _configuration;
        private Helper _helper;
        private IMapper _mapper;

        public BillService(GHCWContext context, IConfiguration configuration, Helper helper, IMapper mapper)
        {
            _context = context;
            _helper = helper;
            _configuration = configuration;
            _mapper = mapper;
        }

        public IQueryable<Bill> GetListBill(int? role, int? uId)
        {
            if (role == 5)
            {
                return _context.Bills.Include(t => t.Customer).Include(t => t.Receptionist).Where(t => t.Customer.AccountId == uId && t.IsActive);
            }
            else if (role == 4)
            {
                return _context.Bills.Include(t => t.Customer).Include(t => t.Receptionist).Where(t => t.IsActive);
            }
            else if (role <= 1)
            {
                return _context.Bills.Include(t => t.Customer).Include(t => t.Receptionist);
            }
            else
            {
                return _context.Bills.Where(t => false);
            }
        }

        public async Task<Bill> GetBilltById(int id)
        {
            return await _context.Bills.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateBill(Bill bill)
        {
            _context.Bills.Update(bill);
            await _context.SaveChangesAsync();
        }

        public async Task<(bool isSuccess, string message)> BillActivation(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                return (false, "Hóa đơn không tồn tại.");
            }
            try
            {
                bill.IsActive = !bill.IsActive;
                _context.Bills.Update(bill);
                await _context.SaveChangesAsync();

                return (true, "Thay đổi trạng thái hóa đơn thành công.");
            }
            catch (Exception)
            {
                return (false, "Thay đổi trạng thái hóa đơn thất bại, vui lòng thử lại.");
            }
        }

        public async Task<List<BillDetail>> GetBillDetails(int id)
        {
            return await _context.BillDetails
                .Include(t => t.Bill)
                .Include(s => s.Product)
                .Where(t => t.BillId == id)
                .ToListAsync();
        }

        public async Task<Bill?> SaveBillAsync(Bill bill)
        {
            try
            {
                _context.Bills.Add(bill);
                await _context.SaveChangesAsync();
                return bill;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
