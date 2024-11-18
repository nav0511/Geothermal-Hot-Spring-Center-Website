using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Helpers;
using GHCW_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Services
{
    public class AccountService
    {
        private readonly GHCWContext _context;
        private readonly IMapper _mapper;

        public AccountService(GHCWContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AccountDTO?> CheckActiveStatus(string email)
        {
            var existUser = await _context.Accounts.FirstOrDefaultAsync(u => u.Email == email && u.IsActive == true);
            if (existUser != null)
            {
                var existUserDTO = _mapper.Map<Account, AccountDTO>(existUser);
                return existUserDTO;
            }
            return null;
        }

        public async Task<Account?> CheckAccountExsit(string email)
        {
            var checkUser = await _context.Accounts.FirstOrDefaultAsync(x => x.Email.Equals(email));
            return checkUser;
        }

        public async Task<List<AccountDTO>?> GetUserList()
        {
            var users = await _context.Accounts.ToListAsync();
            if (users != null)
            {
                var userDTOs = _mapper.Map<List<Account>, List<AccountDTO>>(users);
                return userDTOs;
            }
            return null;
        }

        public async Task<AccountDTO?> GetUserProfileById(int uID)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == uID);
            if (user != null)
            {
                var userDTO = _mapper.Map<Account, AccountDTO>(user);
                return userDTO;
            }
            return null;
        }

        public async Task<List<AccountDTO>?> GetEmployeeList()
        {
            var employees = await _context.Accounts.Where(a => a.Role >= 1 && a.Role <= 4).ToListAsync();
            if (employees != null)
            {
                var employeeDTOs = _mapper.Map<List<Account>, List<AccountDTO>>(employees);
                return employeeDTOs;
            }
            return null;
        }

        public async Task<List<AccountDTO>?> GetReceptionList()
        {
            var employees = await _context.Accounts.Where(a => a.Role == 4).ToListAsync();
            if (employees != null)
            {
                var employeeDTOs = _mapper.Map<List<Account>, List<AccountDTO>>(employees);
                return employeeDTOs;
            }
            return null;
        }

        public async Task<List<AccountDTO>?> GetCustomerAccountList()
        {
            var customers = await _context.Accounts.Where(a => a.Role == 5).ToListAsync();
            if (customers != null)
            {
                var customerDTOs = _mapper.Map<List<Account>, List<AccountDTO>>(customers);
                return customerDTOs;
            }
            return null;
        }

        public async Task<(bool isSuccess, string message)> UserActivation(int uid)
        {
            var acc = await _context.Accounts.FindAsync(uid);
            if (acc == null)
            {
                return (false, "Tài khoản không tồn tại.");
            }
            try
            {
                acc.IsActive = !acc.IsActive;
                _context.Accounts.Update(acc);
                await _context.SaveChangesAsync();

                return (true, "Thay đổi trạng thái tài khoản thành công.");
            }
            catch (Exception)
            {
                return (false, "Thay đổi trạng thái tài khoản thất bại, vui lòng thử lại.");
            }
        }

        public async Task<(bool isSuccess, string message)> AddNewUser(AddRequest a)
        {
            try
            {
                var user = _mapper.Map<AddRequest, Account>(a);
                _context.Accounts.Add(user);
                await _context.SaveChangesAsync();
                return (true, "Thêm tài khoản mới thành công.");
            }
            catch (Exception)
            {
                return (false, "Có lỗi trong quá trình thêm tài khoản, vui lòng thử lại.");
            }
        }

        public async Task<(bool isSuccess, string message)> UpdateProfile(UpdateRequest r)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == r.Id);
            if (user == null)
            {
                return (false, "Không tìm thấy tài khoản.");
            }
            try
            {
                _mapper.Map(r, user);
                _context.Accounts.Update(user);
                await _context.SaveChangesAsync();
                return (true, "Cập nhật thông tin thành công.");
            }
            catch (Exception)
            {
                return (false, "Cập nhật thông tin thất bại, vui lòng kiểm tra lại.");
            }
        }

        public async Task<(bool isSuccess, string message)> EditProfile(EditRequest r)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == r.Id);
            if (user == null)
            {
                return (false, "Không tìm thấy tài khoản.");
            }
            try
            {
                _mapper.Map(r, user);
                _context.Accounts.Update(user);
                await _context.SaveChangesAsync();
                return (true, "Cập nhật thông tin thành công.");
            }
            catch (Exception)
            {
                return (false, "Cập nhật thông tin thất bại, vui lòng kiểm tra lại.");
            }
        }
    }
}
