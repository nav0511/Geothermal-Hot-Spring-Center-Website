using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Helpers;
using GHCW_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GHCW_BE.Services
{
    public class AuthenticationService
    {
        private readonly GHCWContext _context;
        private readonly IConfiguration _configuration;
        private Helper _helper;
        private IMapper _mapper;

        public AuthenticationService(GHCWContext context, IConfiguration configuration, Helper helper, IMapper mapper)
        {
            _context = context;
            _helper = helper;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task Register(Account user)
        {
            await _context.Accounts.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        //kiem tra nguoi dung co ton tai hay khong
        //neu co tra ve account, neu khong tra ve null
        public async Task<Account?> CheckAccountExsit(string email)
        {
            var checkUser = await _context.Accounts.FirstOrDefaultAsync(x => x.Email.Equals(email));
            return checkUser;
        }
        public async Task<CustomerDTO?> CheckCustomerExsit(int aId)
        {
            var checkCustomer = await _context.Customers.FirstOrDefaultAsync(x => x.AccountId == aId);
            if (checkCustomer == null)
            {
                return null;
            }
            var customerDTO = _mapper.Map<Customer, CustomerDTO>(checkCustomer);
            return customerDTO;
        }
        public async Task<CustomerDTO?> CheckPhoneExsit(string phone)
        {
            var checkCustomer = await _context.Customers.FirstOrDefaultAsync(x => x.PhoneNumber.Equals(phone));
            if (checkCustomer == null)
            {
                return null;
            }
            var customerDTO = _mapper.Map<Customer, CustomerDTO>(checkCustomer);
            return customerDTO;
        }

        //kiem tra nguoi dung co ton tai va dang hoat dong hay khong
        //neu co tra ve account, neu khong tra ve null
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

        //Kich hoat tai khoan, sau khi kich hoat se xoa code
        public async Task<bool> RedemActivationCode(ActivationCode code)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Email == code.Email && u.ActivationCode == code.Code);
            if (user == null)
            {
                return false;
            }

            user.IsActive = true; // Kích hoạt tài khoản
            user.ActivationCode = null; // Xóa mã kích hoạt sau khi đã sử dụng
            await _context.SaveChangesAsync();
            return true;
        }

        //gửi email gọi đến API kích hoạt email
        public async Task<bool> SendActivationEmail(RegisterDTO registerDTO, string activationLink)
        {
            var emailSettings = _configuration.GetSection("EmailSettings").Get<SendEmailDTO>();

            SendEmailDTO emailDTO = new SendEmailDTO
            {
                FromEmail = emailSettings.FromEmail,
                Password = emailSettings.Password,
                ToEmail = registerDTO.Email,
                Subject = "Kích hoạt tài khoản",
                Body = $"Nhấp vào liên kết <strong><a href='{activationLink}'>này</a></strong> để kích hoạt tài khoản của bạn."
            };

            return await _helper.SendEmail(emailDTO);
        }

        //gửi email chứa mật khẩu mới được tạo
        public async Task<bool> SendNewPasswordEmail(string email, string newPass)
        {
            var emailSettings = _configuration.GetSection("EmailSettings").Get<SendEmailDTO>();

            SendEmailDTO emailDTO = new SendEmailDTO
            {
                FromEmail = emailSettings.FromEmail,
                Password = emailSettings.Password,
                ToEmail = email,
                Subject = "Đặt lại mật khẩu",
                Body = $"Đây là mật khẩu mới của bạn: <strong>{newPass}</strong><br/>Hãy sử dụng mật khẩu này để đăng nhập vào hệ thống.",
            };

            return await _helper.SendEmail(emailDTO);
        }

        public async Task<string> Login(AccountDTO a)
        {
            var key = _configuration["JWT:SecretKey"];
            var issuer = _configuration["JWT:Issuer"];
            var audience = _configuration["JWT:Audience"];
            var expirationMinutes = double.Parse(_configuration["JWT:Expiration"]);
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, a.Email.ToString()),
                new Claim("ID", a.Id.ToString()),
                new Claim("Role", a.Role.ToString())
            };

            return await _helper.GenerateToken(
                key,
                issuer,
                audience,
                expirationMinutes,
                claims);
        }

        public async Task<string> RefTokenGenerator()
        {
            var key = _configuration["JWT:RefSecretKey"];
            var issuer = _configuration["JWT:Issuer"];
            var audience = _configuration["JWT:Audience"];
            var expirationMinutes = double.Parse(_configuration["JWT:RefExpiration"]);
            return await _helper.GenerateToken(
                key,
                issuer,
                audience,
                expirationMinutes
                );
        }

        public bool Validate(string refreshToken)
        {
            TokenValidationParameters tokenValidation = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:RefSecretKey"])),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JWT:Audience"],
                RoleClaimType = "Type",
                ClockSkew = TimeSpan.Zero
            }; ;
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(refreshToken, tokenValidation, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //lấy thông tin user bằng ID
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

        //lấy danh sách user
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

        //lấy danh sách nhân viên role từ 1 - 4
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

        //lấy danh sách lễ tân role = 4
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

        //lấy danh sách khách hàng role = 5
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

        //Đổi password
        public async Task<bool> ChangePassword(int uId, string newPassword)
        {
            var user = await _context.Accounts.FindAsync(uId);
            if (user != null)
            {
                user.Password = newPassword;
                _context.Accounts.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<AccountDTO?> GetAccountByRefreshToken(string refreshToken)
        {
            var acc = await _context.Accounts.FirstOrDefaultAsync(a => a.RefreshToken == refreshToken);
            var accDTO = _mapper.Map<Account, AccountDTO>(acc);
            return accDTO;
        }

        public async Task DeleteRefToken(AccountDTO a)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == a.Id);
            if (user != null)
            {
                user.RefreshToken = null;
                _context.Accounts.Update(user);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<(bool isSuccess, string message)> UpdateRefreshToken(AccountDTO a)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == a.Id);
            if (user == null)
            {
                return (false, "Không tìm thấy tài khoản.");
            }
            user.RefreshToken = a.RefreshToken;
            try
            {
                _context.Accounts.Update(user);
                await _context.SaveChangesAsync();
                return (true, "Cập nhật thông tin thành công.");
            }
            catch (Exception)
            {
                return (false, "Cập nhật thông tin thất bại, vui lòng kiểm tra lại.");
            }
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

        //user tự chỉnh sửa profile của mình
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

        //admin chỉnh sửa thông tin của user
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

        public async Task<List<CustomerDTO>> GetCustomerList()
        {
            var customers = await _context.Customers.ToListAsync();
            var customerDTOs = _mapper.Map<List<Customer>, List<CustomerDTO>>(customers);
            return customerDTOs;
        }

        public async Task<CustomerDTO?> GetCustomerProfileById(int uID)
        {
            var customer = await _context.Customers.Include(c => c.Account).FirstOrDefaultAsync(u => u.Id == uID);
            if (customer != null)
            {
                var userDTO = _mapper.Map<Customer, CustomerDTO>(customer);
                return userDTO;
            }
            return null;
        }

        public async Task<(bool isSuccess, string message)> AddNewCustomer(AddCustomerRequest a)
        {
            try
            {
                var customer = _mapper.Map<AddCustomerRequest, Customer>(a);
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return (true, "Thêm khách hàng mới thành công.");
            }
            catch (Exception)
            {
                return (false, "Có lỗi trong quá trình thêm khách hàng mới, vui lòng thử lại.");
            }
        }

        public async Task<(bool isSuccess, string message)> EditCustomer(CustomerDTO c)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(u => u.Id == c.Id);
            if (customer == null)
            {
                return (false, "Không tìm thấy khách hàng.");
            }
            try
            {
                _mapper.Map(c, customer);
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                return (true, "Cập nhật thông tin thành công.");
            }
            catch (Exception)
            {
                return (false, "Cập nhật thông tin thất bại, vui lòng kiểm tra lại.");
            }
        }

        public async Task<IEnumerable<Ticket?>> GetBookingListById(int uid)
        {
            var tickets = await _context.Tickets.Where(t => t.CustomerId == uid).ToListAsync();
            return tickets;
        }
    }
}
